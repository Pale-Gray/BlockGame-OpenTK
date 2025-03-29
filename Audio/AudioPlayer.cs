using System;
using System.Collections.Generic;
using System.Linq;
using Game.Util;
using NVorbis;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;
using WaveLoader;

namespace Game.Audio;

public class AudioPlayer
{

    private static ALDevice _alDevice;
    private static ALContext _alContext;
    private static List<AudioSource> _playingAudioSources = new();
    private static List<StreamedAudioSource> _playingStreamedAudioSources = new();
    private static Queue<AudioSource> _audioSourceRemovalQueue = new();
    private static Queue<StreamedAudioSource> _streamedAudioSourceRemovalQueue = new();
    private static bool _isMusicPlaying = false;
    public static Vector3 ListenerPosition {
        get => AL.GetListener(ALListener3f.Position);
        set => AL.Listener(ALListener3f.Position, value.X, value.Y, value.Z);
    }

    public static ALDistanceModel ListenerDistanceModel {
        get => AL.GetDistanceModel();
        set => AL.DistanceModel(value);
    }

    public static float ListenerGain {
        get => AL.GetListener(ALListenerf.Gain);
        set => AL.Listener(ALListenerf.Gain, value);
    }

    struct AudioSource {

        public int AudioBuffer;
        public int Source;

        public void Free() {

            AL.DeleteSource(Source);
            AL.DeleteBuffer(AudioBuffer);

        }

    }

    struct StreamedAudioSource {

        public int[] AudioBuffers;
        public int Source;
        public int CurrentPosition;
        public ALFormat Format;
        public int SampleRate;
        public ArraySegment<byte> SoundData;
        public VorbisReader vorbisReader;

        public StreamedAudioSource() {

        }  

        public void Free() {

            AL.DeleteBuffers(AudioBuffers.Length, AudioBuffers);
            AL.DeleteSource(Source);
            if (vorbisReader != null) vorbisReader.Dispose();

        }

    }

    /// <summary>
    /// Polls the <c>AudioPlayer</c> for automatic deletion of sources that are not playing.
    /// </summary>
    public static void Poll() {

        if (_audioSourceRemovalQueue.TryDequeue(out AudioSource source)) 
        {

            if ((ALSourceState)AL.GetSource(source.Source, ALGetSourcei.SourceState) == ALSourceState.Playing) {

                _audioSourceRemovalQueue.Enqueue(source);

            } else {

                source.Free();
                _playingAudioSources.Remove(source);

            }

        }

        for (int i = 0; i < _playingStreamedAudioSources.Count; i++) {

            StreamedAudioSource streamedAudioSource = _playingStreamedAudioSources[i];

            if ((ALSourceState)AL.GetSource(streamedAudioSource.Source, ALGetSourcei.SourceState) == ALSourceState.Playing) {

                _streamedAudioSourceRemovalQueue.Enqueue(streamedAudioSource);
                int buffersProcessed = AL.GetSource(streamedAudioSource.Source, ALGetSourcei.BuffersProcessed);

                if (buffersProcessed <= 0) return;

                while (buffersProcessed-- > 0) {

                    if (streamedAudioSource.vorbisReader == null) {

                        byte[] data = new byte[_bytesPerBuffer];
                        int buffer = AL.SourceUnqueueBuffer(streamedAudioSource.Source);
                        int dataSizeToCopy = _bytesPerBuffer;
                        if (streamedAudioSource.CurrentPosition + _bytesPerBuffer > streamedAudioSource.SoundData.Count) dataSizeToCopy = streamedAudioSource.SoundData.Count - streamedAudioSource.CurrentPosition;

                        Array.Copy(streamedAudioSource.SoundData.Array, streamedAudioSource.CurrentPosition, data, 0, dataSizeToCopy);
                        streamedAudioSource.CurrentPosition += dataSizeToCopy;

                        if (dataSizeToCopy >= _bytesPerBuffer) {

                            _playingStreamedAudioSources[i] = streamedAudioSource;
                            AL.BufferData(buffer, streamedAudioSource.Format, data, streamedAudioSource.SampleRate);
                            AL.SourceQueueBuffer(streamedAudioSource.Source, buffer);

                        }

                    } else {

                        float[] data = new float[streamedAudioSource.vorbisReader.Channels * streamedAudioSource.vorbisReader.SampleRate / 2];
                        int buffer = AL.SourceUnqueueBuffer(streamedAudioSource.Source);

                        if (!streamedAudioSource.vorbisReader.IsEndOfStream) {

                            streamedAudioSource.vorbisReader.ReadSamples(data, 0, streamedAudioSource.vorbisReader.Channels * streamedAudioSource.vorbisReader.SampleRate / 2);

                            _playingStreamedAudioSources[i] = streamedAudioSource;
                            AL.BufferData(buffer, streamedAudioSource.Format, data, streamedAudioSource.SampleRate);
                            AL.SourceQueueBuffer(streamedAudioSource.Source, buffer);

                        }
                        

                    }

                }
            
            } else {

                // Console.WriteLine("Done playing, freeing.");
                _isMusicPlaying = false;
                streamedAudioSource.Free();
                _playingStreamedAudioSources.Remove(streamedAudioSource);

            }

        }

    }

    /// <summary>
    /// Sets the orientation of the listener
    /// </summary>
    /// <param name="lookAt">The direction the listener is looking</param>
    /// <param name="up">The direction where the up coordinate is</param>
    public static void SetOrientation(Vector3 lookAt, Vector3 up) {

        // you might need these.
        // lookAt *= (1, 1, -1);
        // up *= (1, 1, -1);
        AL.Listener(ALListenerfv.Orientation, ref lookAt, ref up);

    }

    private static int _bufferAmount = 4;
    private static int _bytesPerBuffer = 65536;
    public static void PlayMusicLocal(string filePath, float pitch = 1.0f, float gain = 1.0f) {

        if (!_isMusicPlaying) {

            _isMusicPlaying = true;

            string fileExtension = filePath.Split('.').Last().ToLower();

            if (fileExtension == "wav") {

                WaveFile wav = WaveFile.Load(filePath);
                StreamedAudioSource streamedAudioSource = new StreamedAudioSource();
                ALFormat format = ALFormat.Mono8;
                if (wav.BitsPerSample == 8) format = ALFormat.Stereo8;
                if (wav.BitsPerSample == 16) format = ALFormat.Stereo16;

                streamedAudioSource.AudioBuffers = AL.GenBuffers(_bufferAmount);

                for (int i = 0; i < streamedAudioSource.AudioBuffers.Length; i++) {

                    AL.BufferData<byte>(streamedAudioSource.AudioBuffers[i], format, wav.Data.AsSpan(i * _bytesPerBuffer, _bytesPerBuffer), wav.Channels == 1 ? wav.SampleRate / 2 : wav.SampleRate);

                }

                streamedAudioSource.Source = AL.GenSource();
                AL.Source(streamedAudioSource.Source, ALSourcef.Pitch, pitch);
                AL.Source(streamedAudioSource.Source, ALSourcef.Gain, gain);
                AL.Source(streamedAudioSource.Source, ALSource3f.Position, 0, 0, 0);
                AL.Source(streamedAudioSource.Source, ALSource3f.Velocity, 0, 0, 0);
                AL.Source(streamedAudioSource.Source, ALSourceb.Looping, false);
            
                AL.SourceQueueBuffers(streamedAudioSource.Source, streamedAudioSource.AudioBuffers.Length, streamedAudioSource.AudioBuffers);
                streamedAudioSource.Format = format;
                streamedAudioSource.SampleRate = wav.Channels == 1 ? wav.SampleRate / 2 : wav.SampleRate;
                streamedAudioSource.CurrentPosition = _bufferAmount * _bytesPerBuffer;
                streamedAudioSource.SoundData = wav.Data;
                AL.SourcePlay(streamedAudioSource.Source);
                _playingStreamedAudioSources.Add(streamedAudioSource);
                _streamedAudioSourceRemovalQueue.Enqueue(_playingStreamedAudioSources[_playingStreamedAudioSources.IndexOf(streamedAudioSource)]);

            } else if (fileExtension == "ogg") {

                VorbisReader vorbis = new VorbisReader(filePath);

                StreamedAudioSource streamedAudioSource = new StreamedAudioSource();
                ALFormat format = ALFormat.StereoFloat32Ext;

                streamedAudioSource.AudioBuffers = AL.GenBuffers(_bufferAmount);
                for (int i = 0; i < streamedAudioSource.AudioBuffers.Length; i++) {

                    float[] samples = new float[vorbis.Channels * vorbis.SampleRate / 2];
                    vorbis.ReadSamples(samples, 0, vorbis.Channels * vorbis.SampleRate / 2);
                    AL.BufferData(streamedAudioSource.AudioBuffers[i], format, samples, vorbis.Channels == 1 ? vorbis.SampleRate / 2 : vorbis.SampleRate);

                }

                streamedAudioSource.Source = AL.GenSource();
                AL.Source(streamedAudioSource.Source, ALSourcef.Pitch, pitch);
                AL.Source(streamedAudioSource.Source, ALSourcef.Gain, gain);
                AL.Source(streamedAudioSource.Source, ALSource3f.Position, 0, 0, 0);
                AL.Source(streamedAudioSource.Source, ALSource3f.Velocity, 0, 0, 0);
                AL.Source(streamedAudioSource.Source, ALSourceb.Looping, false);

                AL.SourceQueueBuffers(streamedAudioSource.Source, streamedAudioSource.AudioBuffers.Length, streamedAudioSource.AudioBuffers);
                streamedAudioSource.Format = format;
                streamedAudioSource.SampleRate = vorbis.Channels == 1 ? vorbis.SampleRate / 2 : vorbis.SampleRate;
                streamedAudioSource.vorbisReader = vorbis;
                AL.SourcePlay(streamedAudioSource.Source);
                _playingStreamedAudioSources.Add(streamedAudioSource);
                _streamedAudioSourceRemovalQueue.Enqueue(_playingStreamedAudioSources[_playingStreamedAudioSources.IndexOf(streamedAudioSource)]);

            }

        } else{

            GameLogger.Log("Music is currently playing, you can't add a stream right now!", Severity.Warning);

        } 

    }

    /// <summary>
    /// Plays a sound file globally (3D) without streaming.
    /// </summary>
    /// <param name="filePath">The audio file</param>
    /// <param name="position">The position of the audio source</param>
    /// <param name="direction">The direction of the audio source</param>
    /// <param name="pitch">The pitch modifier</param>
    /// <param name="gain">The gain modifier</param>
    public static void PlaySoundGlobal(string filePath, in Vector3 position, in Vector3 direction = default, float pitch = 1.0f, float gain = 1.0f) {

        string fileExtension = filePath.Split('.').Last().ToLower();

        int audioBuffer = AL.GenBuffer();

        if (fileExtension == "ogg") {

            using (VorbisReader vorbis = new VorbisReader(filePath)) {

                float[] buffer = new float[vorbis.TotalSamples * vorbis.Channels];
                vorbis.ReadSamples(buffer, 0, (int) vorbis.TotalSamples * vorbis.Channels);

                AL.BufferData(audioBuffer, ALFormat.MonoFloat32Ext, buffer, vorbis.Channels == 2 ? vorbis.SampleRate * 2 : vorbis.SampleRate);

                int source = AL.GenSource();
                AL.Source(source, ALSourcef.Pitch, pitch);
                AL.Source(source, ALSourcef.Gain, gain);
                AL.Source(source, ALSource3f.Position, position.X, position.Y, position.Z);
                AL.Source(source, ALSource3f.Velocity, 0, 0, 0);
                AL.Source(source, ALSourceb.Looping, false);
                AL.Source(source, ALSourcei.Buffer, audioBuffer);

                AL.SourcePlay(source);
                AudioSource audioSource = new AudioSource() { Source = source, AudioBuffer = audioBuffer };
                _playingAudioSources.Add(audioSource);
                _audioSourceRemovalQueue.Enqueue(_playingAudioSources[_playingAudioSources.IndexOf(audioSource)]);

            }

        } else if (fileExtension == "wav") {

            WaveFile wav = WaveFile.Load(filePath);
            ALFormat format = ALFormat.Mono8;

            // we only care about mono audio for 3d sound.
            if (wav.BitsPerSample == 8) format = ALFormat.Mono8;
            if (wav.BitsPerSample == 16) format = ALFormat.Mono16;
            
            AL.BufferData<byte>(audioBuffer, format, wav.Data.AsSpan(0, wav.Data.Count), wav.Channels == 2 ? wav.SampleRate * 2 : wav.SampleRate);

            int source = AL.GenSource();
            AL.Source(source, ALSourcef.Pitch, pitch);
            AL.Source(source, ALSourcef.Gain, gain);
            AL.Source(source, ALSource3f.Position, position.X, position.Y, position.Z);
            AL.Source(source, ALSource3f.Velocity, 0, 0, 0);
            AL.Source(source, ALSourceb.Looping, false);
            AL.Source(source, ALSourcei.Buffer, audioBuffer);

            AL.SourcePlay(source);
            AudioSource audioSource = new AudioSource() { Source = source, AudioBuffer = audioBuffer };
            _playingAudioSources.Add(audioSource);
            _audioSourceRemovalQueue.Enqueue(_playingAudioSources[_playingAudioSources.IndexOf(audioSource)]);

        }

    }

    /// <summary>
    /// Plays a sound file locally (2D) without streaming.
    /// </summary>
    /// <param name="filePath">The audio file</param>
    /// <param name="pitch">The pitch modifier</param>
    /// <param name="gain">The gain modifier</param>
    public static void PlaySoundLocal(string filePath, float pitch = 1.0f, float gain = 1.0f)
    {

        string fileExtension = filePath.Split('.').Last().ToLower();

        Span<byte> data = Span<byte>.Empty;
        ALFormat format = ALFormat.Stereo8;
        int audioBuffer = AL.GenBuffer();
        int channels = 0;
        int sampleRate = 0;

        if (fileExtension == "ogg") {

            using (VorbisReader vorbis = new VorbisReader(filePath)) {

                channels = vorbis.Channels;
                sampleRate = vorbis.SampleRate;
                float[] buffer = new float[vorbis.TotalSamples * channels];
                vorbis.ReadSamples(buffer, 0, (int) vorbis.TotalSamples * channels);

                AL.BufferData(audioBuffer, ALFormat.StereoFloat32Ext, buffer, channels == 1 ? sampleRate / 2 : sampleRate);

                int source = AL.GenSource();
                AL.Source(source, ALSourcef.Pitch, pitch);
                AL.Source(source, ALSourcef.Gain, gain);
                AL.Source(source, ALSource3f.Position, 0, 0, 0);
                AL.Source(source, ALSource3f.Velocity, 0, 0, 0);
                AL.Source(source, ALSourceb.Looping, false);
                AL.Source(source, ALSourcei.Buffer, audioBuffer);

                AL.SourcePlay(source);
                AudioSource audioSource = new AudioSource() { Source = source, AudioBuffer = audioBuffer };
                _playingAudioSources.Add(audioSource);
                _audioSourceRemovalQueue.Enqueue(_playingAudioSources[_playingAudioSources.IndexOf(audioSource)]);

            }

        } else if (fileExtension == "wav") {

            WaveFile wav = WaveFile.Load(filePath);
            // stereo sound doesn't handle distance falloff functions, so its good for local sounds!
            if (wav.BitsPerSample == 8) format = ALFormat.Stereo8;
            if (wav.BitsPerSample == 16) format = ALFormat.Stereo16;
            data = wav.Data.AsSpan(0, wav.Data.Count);
            channels = wav.Channels;
            sampleRate = wav.SampleRate;

            AL.BufferData<byte>(audioBuffer, format, data, channels == 1 ? sampleRate / 2 : sampleRate);

            int source = AL.GenSource();
            AL.Source(source, ALSourcef.Pitch, pitch);
            AL.Source(source, ALSourcef.Gain, gain);
            AL.Source(source, ALSource3f.Position, 0, 0, 0);
            AL.Source(source, ALSource3f.Velocity, 0, 0, 0);
            AL.Source(source, ALSourceb.Looping, false);
            AL.Source(source, ALSourcei.Buffer, audioBuffer);

            AL.SourcePlay(source);
            AudioSource audioSource = new AudioSource() { Source = source, AudioBuffer = audioBuffer };
            _playingAudioSources.Add(audioSource);
            _audioSourceRemovalQueue.Enqueue(_playingAudioSources[_playingAudioSources.IndexOf(audioSource)]);

        } else {

            GameLogger.ThrowError("Could not load file. Only WAV and OGG are supported.");

        }

    }

    /// <summary>
    /// Initializes the <c>AudioPlayer</c>
    /// </summary>
    public static void Initialize()
    {
        
        string deviceSpecifier = null;
        foreach (string devices in ALC.GetStringList(GetEnumerationStringList.DeviceSpecifier)) {
            if (devices.Contains("OpenAL Soft")) {
                deviceSpecifier = devices;
                break;
            }
        }
        if (deviceSpecifier == null) {
            GameLogger.ThrowError("Could not find a device with openal soft! Do you have it installed?");
        }
        _alDevice = ALC.OpenDevice(deviceSpecifier);
        if (_alDevice == ALDevice.Null) GameLogger.ThrowError("Something went wrong opening the AL device.");

        _alContext = ALC.CreateContext(_alDevice, Array.Empty<int>());
        if (_alContext == ALContext.Null) GameLogger.ThrowError("Something went wrong creating the AL context.");

        ALC.MakeContextCurrent(_alContext);

    }

    /// <summary>
    /// Cleans up audio sources, etc
    /// </summary>
    public static void Unload() 
    {

        foreach (AudioSource source in _playingAudioSources) {

            source.Free();

        }

        foreach (StreamedAudioSource source in _playingStreamedAudioSources) {

            source.Free();

        }

        ALC.DestroyContext(_alContext);
        ALC.CloseDevice(_alDevice);

    }
    
}