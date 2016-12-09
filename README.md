# Unity Volumetric Explosion Shader

![alt tag](http://www.giphy.com/gifs/l0MYyK3w0n1r71VdK)

I've always thought explosion effects were a weak point in action videogames. They're typically either a sprite animation thrown on some billboarded quads, or an underwhelming particle effect. So I figured I'd set out for something a bit different.

This effect is fairly simple. It consists of a main fireball and multiple smaller effects. The main fireball is achieved through modulating the vertices and per-pixel surface color based on a noise texture lookup and displacing the vertices of the standard sphere geometry in Unity. Here's the breakdown:
+ Vertex displacement is achieved through a texture lookup on an RGB Worley Noise texture. Worley noise is ideal, as it results in a nice bubbly-looking shape. That's what we want our explosion to look like.
+ The results of the texture lookup is combined with a random value that changes over the lifetime of the explosion. Now the explosion has some animation and life to it, and is no longer boring and motionless.
+ Color modulation is achieved through multiple texture lookups, combining the results of the Noise texture and the gradient texture. This results in the bubbly-looking shape being uniformly colored with the points of furthest displacement one end of the gradient texture, and the points of least displacement on the other end of the gradient texture.
+ Other various effects are achieved through public variables exposed in the accompanying script - such as the change in explosion size over its lifetime, the degree of variation/bumpiness it has, whether it spawns sub-explosions, and so forth.

Have fun!
