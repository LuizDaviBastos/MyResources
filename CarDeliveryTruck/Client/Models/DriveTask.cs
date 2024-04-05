using CitizenFX.Core;

namespace CarDeliveryTruck.Client.Models
{
    public class DriveTask
    {
        public Vector3 To { get; set; }
        public Vehicle Vehicle { get; set; }
        public Ped Driver { get; set; }
        public float Range { get; set; } = 1f;

        public DriveTask(Vector3 to, Vehicle vehicle, Ped driver)
        {
            To = to;
            Vehicle = vehicle;
            Driver = driver;
        }

        public DriveTask(Vector3 to, Vehicle vehicle, Ped driver, float range)
        {
            To = to;
            Vehicle = vehicle;
            Driver = driver;
            Range = range;
        }
    }
}
