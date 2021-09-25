using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace User.NearlyOnPace
{
    class SharedMemoryReader
    {
        private enum AC_MEMORY_STATUS { DISCONNECTED, CONNECTING, CONNECTED }

        private MemoryMappedFile graphicsMMF;
        private MemoryMappedFile physicsMMF;
        private AC_MEMORY_STATUS status = AC_MEMORY_STATUS.DISCONNECTED;

        public bool isConnected()
        {
            return status == AC_MEMORY_STATUS.CONNECTED && graphicsMMF != null && physicsMMF != null;
        }

        public SharedMemoryReader()
        {
            ConnectToSharedMemory();
        }

        private bool ConnectToSharedMemory()
        {
            try
            {
                status = AC_MEMORY_STATUS.CONNECTING;
                // Connect to shared memory
                graphicsMMF = MemoryMappedFile.OpenExisting("Local\\acpmf_graphics");
                physicsMMF = MemoryMappedFile.OpenExisting("Local\\acpmf_physics");

                status = AC_MEMORY_STATUS.CONNECTED;
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        public Graphics? readGraphics()
        {
            if (!isConnected())
            {
                ConnectToSharedMemory();

                if(!isConnected())
                {
                    return null;
                }
            }

            var size = Marshal.SizeOf(typeof(Graphics));
            using (var stream = graphicsMMF.CreateViewStream(0, size))
            {
                using (var reader = new BinaryReader(stream))
                {
                    var bytes = reader.ReadBytes(size);
                    var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                    var data = (Graphics)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Graphics));
                    handle.Free();
                    return data;
                }
            }
        }

        public Physics? readPhysics()
        {
            if (!isConnected())
            {
                ConnectToSharedMemory();

                if (!isConnected())
                {
                    return null;
                }
            }

            var size = Marshal.SizeOf(typeof(Physics));
            using (var stream = physicsMMF.CreateViewStream(0, size))
            {
                using (var reader = new BinaryReader(stream))
                {
                    var bytes = reader.ReadBytes(size);
                    var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                    var data = (Physics)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Physics));
                    handle.Free();
                    return data;
                }
            }
        }
    }
}
