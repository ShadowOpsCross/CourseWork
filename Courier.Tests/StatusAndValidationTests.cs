using Courier.Core.Enums;
using Courier.Core.Exceptions;
using Courier.Core.Models;
using Courier.Core.Repositories;
using Courier.Core.Services;
using Xunit;

namespace Courier.Tests
{
    public class StatusAndValidationTests
    {
        private static Parcel CreateParcel() =>
            new ParcelService(new ParcelRepository(), new AddressDirectory())
                .CreateParcel(TestData.CreateSender(), TestData.CreateRecipient(),
                    "Книги", 2, 20, 20, 20, DeliveryType.Standard);

        [Fact]
        public void NewParcel_HasCreatedStatusAndHistory()
        {
            Parcel parcel = CreateParcel();
            Assert.Equal(ParcelStatus.Created, parcel.Status);
            Assert.Single(parcel.History);
        }

        [Fact]
        public void FullHappyPath_ReachesDelivered()
        {
            Parcel parcel = CreateParcel();
            parcel.ChangeStatus(ParcelStatus.Accepted);
            parcel.ChangeStatus(ParcelStatus.InTransit);
            parcel.ChangeStatus(ParcelStatus.OutForDelivery);
            parcel.ChangeStatus(ParcelStatus.Delivered);
            Assert.Equal(ParcelStatus.Delivered, parcel.Status);
            Assert.Equal(5, parcel.History.Count);
        }

        [Theory]
        [InlineData(ParcelStatus.Created, ParcelStatus.Delivered)]
        [InlineData(ParcelStatus.Created, ParcelStatus.InTransit)]
        public void SkippingStatuses_Throws(ParcelStatus from, ParcelStatus to)
        {
            Parcel parcel = CreateParcel();
            Assert.Equal(from, parcel.Status);
            var ex = Assert.Throws<InvalidStatusTransitionException>(() => parcel.ChangeStatus(to));
            Assert.Equal(from, ex.From);
            Assert.Equal(to, ex.To);
        }

        [Fact]
        public void DeliveredParcel_CannotChangeStatus()
        {
            Parcel parcel = CreateParcel();
            parcel.ChangeStatus(ParcelStatus.Accepted);
            parcel.ChangeStatus(ParcelStatus.InTransit);
            parcel.ChangeStatus(ParcelStatus.OutForDelivery);
            parcel.ChangeStatus(ParcelStatus.Delivered);
            Assert.Throws<InvalidStatusTransitionException>(() => parcel.ChangeStatus(ParcelStatus.Returned));
        }

        [Fact]
        public void InTransit_CanBeReturned()
        {
            Parcel parcel = CreateParcel();
            parcel.ChangeStatus(ParcelStatus.Accepted);
            parcel.ChangeStatus(ParcelStatus.InTransit);
            parcel.ChangeStatus(ParcelStatus.Returned);
            Assert.Equal(ParcelStatus.Returned, parcel.Status);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("1")]
        [InlineData("12345")]
        public void InvalidName_Throws(string name)
        {
            Assert.Throws<InvalidParcelDataException>(() => new Sender { FullName = name });
        }

        [Theory]
        [InlineData("123")]
        [InlineData("abcdefghij")]
        [InlineData("")]
        public void InvalidPhone_Throws(string phone)
        {
            Assert.Throws<InvalidParcelDataException>(() => new Sender { Phone = phone });
        }

        [Fact]
        public void Phone_IsNormalized()
        {
            var sender = new Sender { Phone = "+38 (050) 123-45-67" };
            Assert.Equal("+380501234567", sender.Phone);
        }

        [Fact]
        public void InvalidEmail_Throws()
        {
            Assert.Throws<InvalidParcelDataException>(() => new Sender { Email = "not-an-email" });
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(151)]
        public void InvalidDimension_Throws(double value)
        {
            Assert.Throws<InvalidParcelDataException>(() => new Parcel { LengthCm = value });
        }

        [Fact]
        public void Edit_AfterAccepted_IsForbidden()
        {
            var service = new ParcelService(new ParcelRepository(), new AddressDirectory());
            Parcel parcel = service.CreateParcel(TestData.CreateSender(), TestData.CreateRecipient(),
                "Книги", 2, 20, 20, 20, DeliveryType.Standard);
            service.ChangeStatus(parcel.TrackingNumber, ParcelStatus.Accepted);
            Assert.Throws<InvalidParcelDataException>(() =>
                service.EditParcel(parcel.TrackingNumber, "Інше", 3, 10, 10, 10));
        }

        [Fact]
        public void Edit_RecalculatesCost()
        {
            var service = new ParcelService(new ParcelRepository(), new AddressDirectory());
            Parcel parcel = service.CreateParcel(TestData.CreateSender(), TestData.CreateRecipient(),
                "Книги", 2, 10, 10, 10, DeliveryType.Standard);
            decimal before = parcel.Cost;
            service.EditParcel(parcel.TrackingNumber, "Книги", 5, 10, 10, 10);
            Assert.True(parcel.Cost > before);
        }

        [Fact]
        public void GetUnknownParcel_ThrowsNotFound()
        {
            var service = new ParcelService(new ParcelRepository(), new AddressDirectory());
            Assert.Throws<ParcelNotFoundException>(() => service.GetByTrackingNumber("NOPE"));
        }
    }
}
