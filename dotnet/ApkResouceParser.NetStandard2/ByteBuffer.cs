/*
 *	apk resource parser for .net
 *  port of https://github.com/google/android-arscblamer
 *
 *  ikas-mc 2023
 */

using System;
using System.IO;

namespace ApkResourceParser
{
    public class ByteBuffer
    {
        private readonly MemoryStream _stream;
        private readonly BinaryReader _reader;
        private readonly byte[] _data;

        private int _mark = -1;

        public ByteBuffer(byte[] data)
        {
            this._data = data;
            this._stream = new MemoryStream(data);
            this._reader = new BinaryReader(_stream);
        }

        ~ByteBuffer()
        {
            _reader.Dispose();
            _stream.Dispose();
        }

        public int limit()
        {
            return (int)_stream.Length;
        }

        public int position()
        {
            return (int)_stream.Position;
        }

        public void position(int position)
        {
            _stream.Position = position;
        }

        public int remaining()
        {
            return (int)(_stream.Length - _stream.Position);
        }

        public bool hasRemaining()
        {
            return _stream.Position < _stream.Length;
        }

        public byte get()
        {
            return (byte)_stream.ReadByte();
        }

        public byte get(int index)
        {
            long markPosition = _stream.Position;
            _stream.Position = index;
            byte value = _reader.ReadByte();
            _stream.Position = markPosition;
            return value;
        }

        public int get(byte[] dst)
        {
           return _stream.Read(dst, 0, dst.Length);
        }

        public int getInt()
        {
            return _reader.ReadInt32();
        }

        public int getInt(int index)
        {
            long markPosition = _stream.Position;
            _stream.Position = index;
            int value = _reader.ReadInt32();
            _stream.Position = markPosition;
            return value;
        }

        public short getShort()
        {
            return _reader.ReadInt16();
        }

        public short getShort(int index)
        {
            long markPosition = _stream.Position;
            _stream.Position = index;
            short value = _reader.ReadInt16();
            _stream.Position = markPosition;
            return value;
        }

        public byte[] data()
        {
            return _data;
        }

        public void mark()
        {
            _mark = (int)_stream.Position;
        }

        public void reset()
        {
            int m = _mark;
            if (m < 0)
                throw new InvalidOperationException("no mark");
            _stream.Position = _mark;
        }

        public void rewind()
        {
            _stream.Position = 0;
            _mark = -1;
        }
    }
}