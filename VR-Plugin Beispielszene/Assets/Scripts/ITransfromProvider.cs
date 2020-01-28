using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    public interface ITransfromProvider
    {
        Transform Transform { get; }
        bool HasChanged { get; }
        void ResetTransform();
    }
}
