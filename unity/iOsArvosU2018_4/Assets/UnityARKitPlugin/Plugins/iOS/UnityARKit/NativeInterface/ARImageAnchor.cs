using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.iOS
{
    public struct UnityARImageAnchorData
    {
        public IntPtr ptrIdentifier;

        /**
         * The transformation matrix that defines the anchor's rotation, translation and scale in world coordinates.
		 */
        public UnityARMatrix4x4 transform;

        public IntPtr referenceImageNamePtr;
        public float referenceImagePhysicalSize;
        public int isTracked;
    };

    public class ARImageAnchor
    {
        private UnityARImageAnchorData _imageAnchorData;

        public ARImageAnchor(UnityARImageAnchorData uiad)
        {
            _imageAnchorData = uiad;
        }

        public string ReferenceImageName { get { return Marshal.PtrToStringAuto(_imageAnchorData.referenceImageNamePtr); } }
        public float ReferenceImagePhysicalSize { get { return _imageAnchorData.referenceImagePhysicalSize; } }
        public string Identifier { get { return Marshal.PtrToStringAuto(_imageAnchorData.ptrIdentifier); } }
        public bool IsTracked { get { return _imageAnchorData.isTracked != 0; } }

        public Matrix4x4 Transform
        {
            get
            {
                Matrix4x4 matrix = new Matrix4x4();
                matrix.SetColumn(0, _imageAnchorData.transform.column0);
                matrix.SetColumn(1, _imageAnchorData.transform.column1);
                matrix.SetColumn(2, _imageAnchorData.transform.column2);
                matrix.SetColumn(3, _imageAnchorData.transform.column3);
                return matrix;
            }
        }
    }
}