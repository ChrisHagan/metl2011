using System.Collections.Generic;
using SandRibbonObjects;

namespace SandRibbonInterop.Interfaces
{
    public interface IFriendsDisplay
    {
        void SetPopulation(List<Friend> newPopulation);
        void Ping(string who, int where);
        void Join(string who);
        void Flush();
        void Move(string who, int destination);
    }
}
