using Courier.Core.Enums;
using Courier.Core.Exceptions;
using Courier.Core.Models;
using Courier.Core.Repositories;
using Courier.Core.Services;
using Xunit;

namespace Courier.Tests
{
    public class SerializationTests : IDisposable
    {
        private readonly string _path = Path.Combine(Path.GetTempPath(), $"courier_test_{Guid.NewGuid():N}.json");

        public void Dispose()
        {
            if (File.Exists(_path)) File.Delete(_path);
        }

        [Fact]
        public void Load_WhenFileMissing_ReturnsEmptyList()
        {
            Assert.Empty(new JsonFileService<Parcel>(_path).Load());
        }

        [Fact]
        public void SaveThenLoad_RestoresAllData()
        {
            var service = new ParcelService(new ParcelRepository(), new AddressDirectory());
            Parcel original = service.CreateParcel(TestData.CreateSender(), TestData.CreateRecipient(),
                "Ноутбук", 2.5, 40, 30, 10, DeliveryType.Express);
            service.ChangeStatus(original.TrackingNumber, ParcelStatus.Accepted);
            service.ChangeStatus(original.TrackingNumber, ParcelStatus.InTransit);

            var files = new JsonFileService<Parcel>(_path);
            files.Save(service.GetAll());
            Parcel loaded = Assert.Single(files.Load());

            Assert.Equal(original.TrackingNumber, loaded.TrackingNumber);
            Assert.Equal(original.Description, loaded.Description);
            Assert.Equal(original.WeightKg, loaded.WeightKg);
            Assert.Equal(original.Cost, loaded.Cost);
            Assert.Equal(ParcelStatus.InTransit, loaded.Status);
            Assert.Equal(3, loaded.History.Count);
            Assert.Equal(original.CreatedAt, loaded.CreatedAt);
            Assert.Equal(DeliveryType.Express, loaded.Route.Type);
            Assert.Equal(original.Route.DistanceKm, loaded.Route.DistanceKm);
            Assert.Equal(original.Sender.FullName, loaded.Sender.FullName);
            Assert.Equal(original.Sender.Email, loaded.Sender.Email);
            Assert.Equal(original.Recipient.FullAddress, loaded.Recipient.FullAddress);
        }

        [Fact]
        public void SavedJson_ContainsReadableCyrillicAndEnumNames()
        {
            var service = new ParcelService(new ParcelRepository(), new AddressDirectory());
            service.CreateParcel(TestData.CreateSender(), TestData.CreateRecipient(),
                "Книги", 1, 10, 10, 10, DeliveryType.Standard);
            new JsonFileService<Parcel>(_path).Save(service.GetAll());

            string json = File.ReadAllText(_path);
            Assert.Contains("Книги", json);
            Assert.Contains("\"Created\"", json);
        }

        [Fact]
        public void Load_CorruptedJson_ThrowsDataFileCorrupted()
        {
            File.WriteAllText(_path, "{ це не json ");
            Assert.Throws<DataFileCorruptedException>(() => new JsonFileService<Parcel>(_path).Load());
        }

        [Fact]
        public void Load_JsonWithInvalidValues_ThrowsDataFileCorrupted()
        {
            File.WriteAllText(_path, "[{\"WeightKg\": -5}]");
            Assert.Throws<DataFileCorruptedException>(() => new JsonFileService<Parcel>(_path).Load());
        }

        [Fact]
        public void Repository_RejectsDuplicateTrackingNumbers()
        {
            var repo = new ParcelRepository();
            var parcel = new Parcel();
            repo.Add(parcel);
            Assert.Throws<InvalidParcelDataException>(() => repo.Add(new Parcel { TrackingNumber = parcel.TrackingNumber }));
        }
    }
}
