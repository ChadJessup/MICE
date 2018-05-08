# MICE
Multiple Integrated Circuit Emulator - attempt at building emulators by piecing together ICs in C#

This is a personal learning project in regards to emulating multiple devices, starting with NES.  It currently handles a few of the older NES games (mappers would need to be written for more later).

It's pure C#, and attempting to hit full CPU cycle, bug, and pixel accuracy from the standpoint of a NES. It's getting there...since it's shooting for full accuracy and pure C#, it's not the speediest of demons. 60fps is a good day, when then stars align.

If you want a *good* open-source NES emulator, I recommend [Mesen](https://github.com/SourMesen/Mesen) which has a C++ core, and C# debugger/UI layer. It should be near 100% on cycle/pixel/bug accuracy already if that's what you're looking for. The code is excellent, and was a reference for me on some of the trickier bits more than a few times (Thanks!)

MICE is using some of the newest C# features at the time of this writing, so Span<T> and whatnot.
Emulator design is a fascinating subject, and one I've only scratched the surface of, but this project might be interesting for other C# devs who are curious on where to get started.
 
As a way of trying out optimization patterns, I kept multiple versions of certain parts of the core around. So, if you want to see how bad of an idea LINQ is in something like this with ~4million pixels a second, I have that!  If you want to see Memory<T>/Span<T> and a couple inbetweens memory maps, those are here as well.
