using CarDeliveryTruck.Client.Models;
using CitizenFX.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace CarDeliveryTruck.Client
{
    public partial class ClientMain : BaseScript
    {
        private bool pedGiveVehicle = true;
        public void OpenRampa()
        {
            TriggerEvent("open:rampa", Driver.Handle, CarLocalNet);
        }

        public async Task<KeyValuePair<Vehicle, Vehicle>> SpawnTruck(string name = null)
        {
            Model model = string.IsNullOrEmpty(name) ? new Model(VehicleHash.Flatbed) : name;
            Truck = await World.CreateVehicle(model, Game.PlayerPed.Position + new Vector3(5, 0, 0), Game.PlayerPed.Heading);
            CarLocal = await World.CreateVehicle(new Model(VehicleHash.Adder), Game.PlayerPed.Position + new Vector3(10, 0, 0), Game.PlayerPed.Heading);
            Utils.UnlockDoors(CarLocal);
            CarLocalNet = VehToNet(CarLocal.Handle);

            if (name == "flatbed3")
            {
                if (!HasModelLoaded(new Model("inm_flatbed_base")))
                {
                    RequestModel(new Model("inm_flatbed_base"));
                }

                Vector3 a = Vector3.Zero, pos = Vector3.Zero;
                GetEntityMatrix(Truck.Handle, ref a, ref a, ref a, ref pos);
                DecorSetInt(Truck.Handle, "flatbed3_bed", 0);
                var bed = CreateObjectNoOffset(new Model("inm_flatbed_base"), pos.X, pos.Y, pos.Z, true, false, false);
                var bedEntity = Entity.FromHandle(bed);
                bedEntity.AttachTo(Truck, new Vector3(0, -3.5f, 0.4f));

                if (DoesEntityExist(bed))
                {
                    int bedNet = ObjToNet(bed);
                    DecorSetInt(Truck.Handle, "flatbed3_bed", bedNet);
                }

                CarLocal.AttachTo(bedEntity, new Vector3(0, 2, 0.7f));

                DecorSetInt(Truck.Handle, "flatbed3_car", CarLocalNet);
                DecorSetBool(Truck.Handle, "flatbed3_attached", true);
                DecorSetInt(Truck.Handle, "flatbed3_state", 1);
                
            }
            else
            {
                CarLocal.AttachTo(Truck, new Vector3(0, -2, 1));
            }

            Driver = await World.CreatePed(PedHash.PrologueHostage01, Truck.Position, Truck.Heading);
            Driver.SetIntoVehicle(Truck, VehicleSeat.Driver);

            return new KeyValuePair<Vehicle, Vehicle>(Truck, CarLocal);
        }

        public bool GetWaypointCoords(out Vector3 coords)
        {
            var waypoint = GetFirstBlipInfoId(8);
            coords = GetBlipCoords(waypoint);
            coords.Z = 0;
            return waypoint > 0;
        }

        public void DeleteTruck()
        {
            var vehicle = Truck.Handle;
            var driver = Driver.Handle;
            DeleteVehicle(ref vehicle);
            DeletePed(ref driver);
        }

        public async Task CheckPedGiveVehicle()
        {
            if (Truck == null || CarLocal == null) return;

            float distance = Utils.GetDistance(Truck.Position, CarLocal.Position);
            if(distance > 10 && pedGiveVehicle)
            {
                pedGiveVehicle = false;
                TaskDriveManager.OnAllTasksDone -= OpenRampa;
                TaskDriveManager.OnAllTasksDone += DeleteTruck;
                TaskDriveManager.AddTaskTo(new DriveTask(new Vector3(95f, -1050f, 0), Truck, Driver));
            }
        }
    }
}