using Courier.Core.Enums;
using Courier.Core.Exceptions;
using Courier.Core.Pricing;
using Courier.Core.Repositories;
using Courier.Core.Services;
using Xunit;

namespace Courier.Tests
{
    public class PricingTests
    {
        private static ParcelService CreateService() => new(new ParcelRepository(), new AddressDirectory());

        [Fact]
        public void StandardCost_UsesFormula()
        {
            // 2 кг, коробка 10x10x10 (об'ємна вага 0.2 кг), Київ-Львів 540 км
            // 50 + 2*12 + 540*0.6 = 50 + 24 + 324 = 398
            decimal cost = CreateService().EstimateCost("Київ", "Львів", 2, 10, 10, 10, DeliveryType.Standard);
            Assert.Equal(398m, cost);
        }

        [Fact]
        public void ExpressCost_IsMultipliedBy1_5()
        {
            decimal cost = CreateService().EstimateCost("Київ", "Львів", 2, 10, 10, 10, DeliveryType.Express);
            Assert.Equal(597m, cost);
        }

        [Fact]
        public void VolumetricWeight_IsUsedWhenGreaterThanActual()
        {
            // 1 кг, але 50x50x50 = 125000 см3 -> 25 кг об'ємної ваги
            var parcel = CreateService().CreateParcel(TestData.CreateSender(), TestData.CreateRecipient(),
                "Подушки", 1, 50, 50, 50, DeliveryType.Standard);
            Assert.Equal(25, parcel.ChargeableWeightKg, 3);
            // 50 + 25*12 + 324 = 674
            Assert.Equal(674m, parcel.Cost);
        }

        [Fact]
        public void Weight_AboveLimit_ThrowsWeightLimitExceeded()
        {
            var ex = Assert.Throws<WeightLimitExceededException>(() =>
                CreateService().CreateParcel(TestData.CreateSender(), TestData.CreateRecipient(),
                    "Важке", 31, 10, 10, 10, DeliveryType.Standard));
            Assert.Equal(PricingConfig.MaxWeightKg, ex.LimitKg);
            Assert.Equal(31, ex.ActualWeightKg);
        }

        [Fact]
        public void Weight_ExactlyAtLimit_IsAllowed()
        {
            var parcel = CreateService().CreateParcel(TestData.CreateSender(), TestData.CreateRecipient(),
                "Межа", 30, 10, 10, 10, DeliveryType.Standard);
            Assert.Equal(30, parcel.WeightKg);
        }

        [Fact]
        public void UnknownCity_ThrowsAddressNotFound()
        {
            var ex = Assert.Throws<AddressNotFoundException>(() =>
                CreateService().EstimateCost("Київ", "Атлантида", 1, 10, 10, 10, DeliveryType.Standard));
            Assert.Equal("Атлантида", ex.Address);
        }

        [Fact]
        public void Factory_ReturnsCorrectStrategy()
        {
            Assert.IsType<StandardPricingStrategy>(PricingStrategyFactory.Create(DeliveryType.Standard));
            Assert.IsType<ExpressPricingStrategy>(PricingStrategyFactory.Create(DeliveryType.Express));
        }

        [Fact]
        public void Distance_IsSymmetricAndCaseInsensitive()
        {
            var dir = new AddressDirectory();
            Assert.Equal(dir.GetDistanceKm("Київ", "Львів"), dir.GetDistanceKm("львів", "КИЇВ"));
        }
    }
}
