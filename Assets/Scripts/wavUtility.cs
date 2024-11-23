using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    public static byte[] FromAudioClip(AudioClip clip)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            int sampleCount = clip.samples * clip.channels;
            int frequency = clip.frequency;
            int headerSize = 44;
            stream.Write(new byte[headerSize], 0, headerSize);

            float[] samples = new float[sampleCount];
            clip.GetData(samples, 0);

            foreach (float sample in samples)
            {
                short intSample = (short)(sample * 32767);
                byte[] byteArr = BitConverter.GetBytes(intSample);
                stream.Write(byteArr, 0, byteArr.Length);
            }

            byte[] wavBytes = stream.ToArray();
            WriteWavHeader(wavBytes, clip);
            return wavBytes;
        }
    }

    private static void WriteWavHeader(byte[] wavBytes, AudioClip clip)
    {
        int fileSize = wavBytes.Length - 8;
        int frequency = clip.frequency;
        int channels = clip.channels;
        int sampleCount = clip.samples * channels;
        int byteRate = frequency * channels * 2;

        Buffer.BlockCopy(BitConverter.GetBytes(0x46464952), 0, wavBytes, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(fileSize), 0, wavBytes, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(0x45564157), 0, wavBytes, 8, 4);

        Buffer.BlockCopy(BitConverter.GetBytes(0x20746D66), 0, wavBytes, 12, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(16), 0, wavBytes, 16, 4);
        Buffer.BlockCopy(BitConverter.GetBytes((short)1), 0, wavBytes, 20, 2);
        Buffer.BlockCopy(BitConverter.GetBytes((short)channels), 0, wavBytes, 22, 2);
        Buffer.BlockCopy(BitConverter.GetBytes(frequency), 0, wavBytes, 24, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(byteRate), 0, wavBytes, 28, 4);
        Buffer.BlockCopy(BitConverter.GetBytes((short)(channels * 2)), 0, wavBytes, 32, 2);
        Buffer.BlockCopy(BitConverter.GetBytes((short)16), 0, wavBytes, 34, 2);

        Buffer.BlockCopy(BitConverter.GetBytes(0x61746164), 0, wavBytes, 36, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(sampleCount * 2), 0, wavBytes, 40, 4);
    }
}
