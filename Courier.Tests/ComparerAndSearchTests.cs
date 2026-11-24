using Courier.Core.Comparers;
using Courier.Core.Enums;
using Courier.Core.Models;
using Courier.Core.Repositories;
using Courier.Core.Services;
using Xunit;

namespace Courier.Tests
{
    public class ComparerAndSearchTests
    {
        private static ParcelService CreateServiceWithData()
        {
            var service = new ParcelService(new ParcelRepository(), new AddressDirectory());
            service.CreateParcel(TestData.CreateSender("Київ"), TestData.CreateRecipient("Львів"),
                "Легка", 1, 10, 10, 10, DeliveryType.Standard);
            service.CreateParcel(TestData.CreateSender("Київ"), TestData.CreateRecipient("Одеса"),
                "Середня", 10, 10, 10, 10, DeliveryType.Express);
            service.CreateParcel(TestData.CreateSender("Харків"), TestData.CreateRecipient("Київ"),
                "Важка", 25, 10, 10, 10, DeliveryType.Standard);
            return service;
        }

        [Fact]
        public void WeightComparer_SortsAscendingAndDescending()
        {
            var service = CreateServiceWithData();
            var asc = service.GetSorted(new ParcelWeightComparer());
            var desc = service.GetSorted(new ParcelWeightComparer(), descending: true);
            Assert.Equal(new[] { 1.0, 10.0, 25.0 }, asc.Select(p => p.WeightKg));
            Assert.Equal(new[] { 25.0, 10.0, 1.0 }, desc.Select(p => p.WeightKg));
        }

        [Fact]
        public void CostComparer_SortsByCost()
        {
            var sorted = CreateServiceWithData().GetSorted(new ParcelCostComparer());
            Assert.True(sorted.Zip(sorted.Skip(1), (a, b) => a.Cost <= b.Cost).All(x => x));
        }

        [Fact]
        public void StatusComparer_SortsByLifecycleOrder()
        {
            var service = CreateServiceWithData();
            var first = service.GetAll()[2];
            service.ChangeStatus(first.TrackingNumber, ParcelStatus.Accepted);
            var sorted = service.GetSorted(new ParcelStatusComparer(), descending: true);
            Assert.Equal(first.TrackingNumber, sorted[0].TrackingNumber);
        }

        [Fact]
        public void Comparers_HandleNulls()
        {
            IComparer<Parcel> comparer = new ParcelWeightComparer();
            Assert.Equal(0, comparer.Compare(null, null));
            Assert.True(comparer.Compare(null, new Parcel()) < 0);
            Assert.True(comparer.Compare(new Parcel(), null) > 0);
        }

        [Fact]
        public void FindByCity_MatchesOriginAndDestination()
        {
            Assert.Equal(3, CreateServiceWithData().FindByCity("київ").Count);
            Assert.Single(CreateServiceWithData().FindByCity("Одеса"));
        }

        [Fact]
        public void FindByWeightRange_FiltersCorrectly()
        {
            Assert.Single(CreateServiceWithData().FindByWeightRange(20, 30));
        }

        [Fact]
        public void FindByStatus_FiltersCorrectly()
        {
            var service = CreateServiceWithData();
            service.ChangeStatus(service.GetAll()[0].TrackingNumber, ParcelStatus.Accepted);
            Assert.Single(service.FindByStatus(ParcelStatus.Accepted));
            Assert.Equal(2, service.FindByStatus(ParcelStatus.Created).Count);
        }

        [Fact]
        public void FindByPersonName_IsCaseInsensitive()
        {
            Assert.Equal(3, CreateServiceWithData().FindByPersonName("петренко").Count);
        }

        [Fact]
        public void Statistics_AreCorrect()
        {
            var stats = CreateServiceWithData().GetStatistics();
            Assert.Equal(3, stats.TotalCount);
            Assert.Equal(12.0, stats.AverageWeightKg, 3);
            Assert.Equal(3, stats.CountByStatus[ParcelStatus.Created]);
            Assert.True(stats.TotalCost > 0);
        }

        [Fact]
        public void Delete_RemovesParcel()
        {
            var service = CreateServiceWithData();
            service.DeleteParcel(service.GetAll()[0].TrackingNumber);
            Assert.Equal(2, service.GetAll().Count);
        }
    }
}
