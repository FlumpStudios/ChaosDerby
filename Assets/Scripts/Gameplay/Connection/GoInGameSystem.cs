using Unity.Burst;
using Unity.Collections;
using Unity.Entities.Racing.Common;
using Unity.NetCode;
using UnityEngine;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    ///  Place any established network connection in-game so ghost snapshot sync can start
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation |
                       WorldSystemFilterFlags.ServerSimulation |
                       WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct GoInGameSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AutoConnect>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            FixedString32Bytes worldName = state.World.Name;

            // Go in game as soon as we have a connection set up (connection network ID has been set)
            foreach (var (networkId, entity) 
                     in Query<RefRO<NetworkId>>().WithNone<NetworkStreamInGame>().WithEntityAccess())
            {
                Debug.Log($"[{worldName}] Go in game connection {networkId.ValueRO.Value}");
                commandBuffer.AddComponent<NetworkStreamInGame>(entity);
            }

            commandBuffer.Playback(state.EntityManager);
            commandBuffer.Dispose();
        }
    }
}