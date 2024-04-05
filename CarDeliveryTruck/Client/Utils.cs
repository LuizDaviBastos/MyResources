using CitizenFX.Core;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace CarDeliveryTruck.Client
{
    public class Utils
    {
        public static void DrawText3D(Vector3 vector3, string text)
        {
            DrawText3D(vector3.X, vector3.Y, vector3.Z, text);
            DecorSetInt(1, "", VehToNet(1));
                
        }

        public static void DrawText3D(float x, float y, float z, string text)
        {
            Vector3? onScreenCoords = World3DToScreen2D(x, y, z);

            if (onScreenCoords != null)
            {
                float _x = onScreenCoords.Value.X;
                float _y = onScreenCoords.Value.Y;

                // Configurar o texto
                SetTextScale(0.28f, 0.28f);
                SetTextFont(4);
                SetTextProportional(true);
                SetTextColour(255, 255, 255, 215);
                SetTextEntry("STRING");
                SetTextCentre(true);
                AddTextComponentString(text);

                // Desenhar o texto
                DrawText(_x, _y);

                // Calcular o tamanho do retângulo baseado no tamanho do texto
                float factor = (text.Length) / 370.0f;

                // Desenhar o retângulo de fundo para o texto
                DrawRect(_x, _y + 0.0125f, 0.005f + factor, 0.03f, 41, 11, 41, 68);
            }
        }

        // Função para converter as coordenadas 3D do mundo para coordenadas 2D na tela
        private static Vector3? World3DToScreen2D(float x, float y, float z)
        {
            float screenX = 0, screenY = 0;
            bool result = World3dToScreen2d(x, y, z, ref screenX, ref screenY);
            if (result) return new Vector3(screenX, screenY, 0f);
            else return null;
        }

        public static async Task SpawnCar(string carModel = "adder", Action<Vehicle> onSpawn = null)
        {
            var vehicle = await World.CreateVehicle(carModel, Game.PlayerPed.Position, Game.PlayerPed.Heading);
            Game.PlayerPed.SetIntoVehicle(vehicle, VehicleSeat.Driver);
            if (onSpawn != null)
            {
                onSpawn(vehicle);
            }
        }

        public static Vector3 UpdateRelativePosition(Vehicle vehicle, float uni = 10.0f)
        {
            // Obtém a posição e a rotação do veículo
            Vector3 vehiclePosition = vehicle.Position;
            Vector3 vehicleRotation = vehicle.Rotation;

            // Converte a rotação do veículo de ângulos de Euler (graus) para radianos

            float headingRadians = MathUtil.DegreesToRadians(vehicleRotation.Z);

            // Calcula a nova posição relativa 10 unidades atrás do veículo
            float offsetX = (float)Math.Sin(headingRadians) * uni;
            float offsetY = (float)Math.Cos(headingRadians) * uni;

            Vector3 relativePosition = new Vector3(
                vehiclePosition.X - offsetX,
                vehiclePosition.Y + offsetY,
                vehiclePosition.Z);

            Debug.WriteLine($"Nova posição relativa: {relativePosition}");
            return relativePosition;
        }

        public static float GetDistance(Vector3 v1, Vector3 v2)
        {
            return Vector2.Distance(new Vector2(Convert.ToInt32(v1.X), Convert.ToInt32(v1.Y)), new Vector2(Convert.ToInt32(v2.X), Convert.ToInt32(v2.Y)));
        }

        public static void UnlockDoors(Vehicle vehicle)
        {
            vehicle.LockStatus = VehicleLockStatus.Unlocked;
        }
    }
}
