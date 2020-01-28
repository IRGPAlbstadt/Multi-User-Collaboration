using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class TransformSync : MonoBehaviour, IGUIDProvider, ITransfromProvider
    {
        [SerializeField]
        private string m_GUID = Guid.NewGuid().ToString();
        public string GUID => m_GUID;

        public Transform Transform => this.transform;

        public bool HasChanged { get; private set; } = false;

        private float lastDistance = 0.0f;
        private float lastAngle = 0.0f; // neu
        const float MIN_DISTANCE_TO_SEND_POSITION = 0.001f;
        const float MIN_ANGLE_TO_SEND_POSITION = 0.001f; //neu
        private Vector3 lastNetworkedPosition = Vector3.zero;
        private Quaternion lastNetworkedRotation = Quaternion.identity; //neu

        public void ResetTransform()
        {
            HasChanged = false;
            lastNetworkedPosition = this.transform.position;
            lastNetworkedRotation = this.transform.rotation;
        }

        public void Update()
        {
            lastDistance = Vector3.Distance(lastNetworkedPosition, this.transform.position);
            var deltaRotation = Quaternion.Inverse(lastNetworkedRotation) * this.transform.rotation; // neu
            var mag = deltaRotation.eulerAngles.magnitude; //Berechnen des Betrags des Euler-Winkels

            if(lastDistance >= MIN_DISTANCE_TO_SEND_POSITION || mag >= MIN_ANGLE_TO_SEND_POSITION)
            {
                HasChanged = true;
            }
        }
    }
}
