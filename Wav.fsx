open System
open System.IO

module Wave =
    /// Sample rate
    let sampleRate = 44100
#if EIGHTBIT
    let sample x = (x + 1.)/2. * 255. |> byte
    let bitsPerSample = 8s
#else
    let bitsPerSample = 16s
    let sample x = x * 32767. |> int16
#endif
    /// Writes WAVE PCM soundfile
    let write stream (data:byte[]) =
        let writer = new BinaryWriter(stream)
        // RIFF
        writer.Write("RIFF"B)
        let size = 36 + data.Length in writer.Write(size)
        writer.Write("WAVE"B)
        // fmt
        writer.Write("fmt "B)
        let headerSize = 16 in writer.Write(headerSize)
        let pcmFormat = 1s in writer.Write(pcmFormat)
        let mono = 1s in writer.Write(mono)
        writer.Write(sampleRate)
        let byteRate = sampleRate * int bitsPerSample / 2 in writer.Write(byteRate)
        let blockAlign = bitsPerSample / 2s in writer.Write(blockAlign)
        writer.Write(bitsPerSample)
        // data
        writer.Write("data"B)
        writer.Write(data.Length)
        writer.Write(data)

let sampleRate = 44100
let sample x = x * 32767. |> int16
let sampleLength duration = duration * float sampleRate |> int

let pi = Math.PI
let sineWave freq i = 
    sin (pi * 2. * float i / float sampleRate * freq)
let fadeOut duration i = 
    let sampleLength = sampleLength duration
    float (sampleLength - i) / float sampleLength
let tremolo freq depth i = (1.0 - depth) + depth * (sineWave freq i) ** 2.0

let toBytes (xs:int16[]) =
    let bytes = Array.CreateInstance(typeof<byte>, 2 * xs.Length)
    Buffer.BlockCopy(xs, 0, bytes, 0, 2*xs.Length)
    bytes :?> byte[]

let create duration f =
    let sampleLength = duration * float sampleRate |> int
    Array.init sampleLength (f >> min 1.0 >> max -1.0 >> sample)
    |> toBytes

let freq = 220.00
let duration = 3.0

let shape i = sineWave freq i * fadeOut duration i * tremolo 500. 0.5 i
let bytes = create duration shape

let stream = File.OpenWrite(@"C:\Gherkin\sound.wav")
Wave.write stream bytes
stream.Close()
