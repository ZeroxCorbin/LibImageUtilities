using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LibImageUtilities.ImageTypes.Bmp
{
    public class FileHeader
    {
        public const int Length = 14;

        private readonly List<byte> _data;
        public byte[] RawData => _data.ToArray();
        public int RawDataLength => _data.Count;

        public static readonly byte[] defaultData =
        [
            //Signature
            0x42, 0x4D,
            //FileSize
            0x00, 0x00, 0x00, 0x00,
            //Reserved1
            0x00, 0x00,
            //Reserved2
            0x00, 0x00,
            //DataOffset
            0x00, 0x00, 0x00, 0x00
        ];

        public bool IsValid => _data.Count == 14 &&
            _data[0] == 0x42 && //ASCII 'B'
            _data[1] == 0x4D;   //ASCII 'M'

        public string Signature => System.Text.Encoding.Default.GetString(_data.Take(2).ToArray());
        public int FileSize
        {
            get => BitConverter.ToInt32(_data.Skip(2).Take(4).ToArray(), 0);
            set
            {
                byte[] sizeBytes = BitConverter.GetBytes(value);
                _data[2] = sizeBytes[0];
                _data[3] = sizeBytes[1];
                _data[4] = sizeBytes[2];
                _data[5] = sizeBytes[3];
            }
        }
        public int Reserved1 => BitConverter.ToInt16(_data.Skip(6).Take(2).ToArray(), 0);
        public int Reserved2 => BitConverter.ToInt16(_data.Skip(8).Take(2).ToArray(), 0);
        public int DataOffset
        {
            get => BitConverter.ToInt32(_data.Skip(10).Take(4).ToArray(), 0);
            set
            {
                byte[] offsetBytes = BitConverter.GetBytes(value);
                _data[10] = offsetBytes[0];
                _data[11] = offsetBytes[1];
                _data[12] = offsetBytes[2];
                _data[13] = offsetBytes[3];
            }
        }

        public FileHeader() => _data = defaultData.ToList();
        public FileHeader(byte[] data) => _data = data.ToList();
        public FileHeader(List<byte> data) => _data = data;
    }
}
