using System.Net.Http;
using System.Xml.Linq;
using System.IO;
using System.Security.AccessControl;
using System;
using Sandbox;
using NVorbis;
using MP3Sharp;
using static Sandbox.Event;
using MP3Sharp.Decoding;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Rhythm4K;

public class MusicStream
{
    public static readonly int ChannelBufferSize = 512;

    public string Path;
    public SoundStream Stream;
	
	public bool Finished => Stream.QueuedSampleCount <= 0;

	VorbisReader readerOgg;
    MP3Stream readerMp3;

    enum STREAM_TYPE { NONE, OGG, MP3 };
    STREAM_TYPE StreamType = STREAM_TYPE.NONE;

    public MusicStream(string path)
    {
        Path = path;

        var fs = FileSystem.Data;
        if(!fs.FileExists(Path))
        {
			Log.Error( $"File {Path} does not exist");
        }

		Log.Info( "Loading music stream for " + Path );

		if (Path.EndsWith(".ogg"))
        {
			Log.Info( ".OGG file detected" );
			LoadOgg();
        }
        else if(Path.EndsWith(".mp3"))
        {
			Log.Info( ".MP3 file detected" );
            LoadMp3();
        }
        else
        {
			Log.Error( $"File {Path} is not a supported audio format");
        }
    }

    private void LoadOgg()
    {
        readerOgg = new VorbisReader(Path);
        StreamType = STREAM_TYPE.OGG;
    }

    public void LoadMp3()
    {
        readerMp3 = new MP3Stream(Path);
        StreamType = STREAM_TYPE.MP3;
    }

    public void Play()
    {
        if(StreamType == STREAM_TYPE.OGG)
        {
            PlayOgg();
        }
        else if(StreamType == STREAM_TYPE.MP3)
        {
            PlayMp3();
        }
    }

    private void PlayOgg()
    {
		Stream = new SoundStream( readerOgg.SampleRate, readerOgg.Channels );

		var bufferSize = readerOgg.Channels * ChannelBufferSize;
		var readBuffer = new float[bufferSize];
		var writeBuffer = new short[bufferSize];


		int cnt;
		int samplesProcessed = 0;
		while ( (cnt = readerOgg.ReadSamples( readBuffer, 0, bufferSize )) > 0 )
		{
			samplesProcessed += cnt;

			for ( int i = 0; i < readBuffer.Length; i += readerOgg.Channels )
			{
				for ( int j = 0; j < readerOgg.Channels; j++ )
				{
					float sample = readBuffer[i + j];
					writeBuffer[i + j] = ConvertSample( sample );
					//deinterleavedData[j][i / readerOgg.Channels] = sample;
				}
			}

			Stream.WriteData( writeBuffer );
		}

		Stream.Play();
	}

    private void PlayMp3()
    {
		Stream = new SoundStream( readerMp3.Frequency, readerMp3.ChannelCount );

		var bufferSize = 4096;
		var readBuffer = new byte[bufferSize];
		var writeBuffer = new short[bufferSize];
		
		// Create the buffer.
		// read the entire mp3 file.
		int bytesReturned = readerMp3.Read( readBuffer, 0, bufferSize);

		// Convert array of bytes to array of shorts
		for ( int i = 0; i < bytesReturned; i += 2 )
		{
			writeBuffer[i / 2] = BitConverter.ToInt16( readBuffer, i );
		}

		Stream.WriteData( writeBuffer );

		// close the stream after we're done with it.
		readerMp3.Close();

		Stream.Play();
	}

    public void Stop()
    {
        Stream?.Delete();
    }

    short ConvertSample( float sample )
	{
		return (short)(sample * short.MaxValue);
	}
}
