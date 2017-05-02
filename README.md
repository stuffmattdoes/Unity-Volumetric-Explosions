# Unity Volumetric Explosion Shader

![alt tag](https://github.com/stuffmattdoes/Unity-Volumetric-Explosions/blob/master/Dec-09-2016%2015-10-47.gif)

I've always thought explosion effects were a weak point in action videogames. They're typically either a sprite animation thrown on some billboarded quads, or an underwhelming particle effect. So I figured I'd set out for something a bit different.

This effect is fairly simple. It consists of a main fireball and multiple smaller effects. The main fireball is achieved through morphing the shape and color of a standard sphere geometry over time. Here's the technical breakdown:

+ Vertex displacement (morphing the shape of the sphere) is achieved through a texture lookup on an RGB Worley Noise texture. THe different RGB values on the texture translate directly to the sphere surface displacement. A worley noise texture is ideal, as it results in a nice bubbly-looking shape. That's what we want our explosion to look like.
+ The results of the texture lookup is combined with a random value that changes over the lifetime of the explosion. Now the explosion has some animation and life to it, and is no longer static and boring.
+ Color modulation is achieved through multiple texture lookups, combining the results of the Noise texture and the gradient texture. This results in the bubbly-looking shape being uniformly colored with the points of furthest displacement one end of the gradient texture, and the points of least displacement on the other end of the gradient texture.
+ Other various effects are achieved through public variables exposed in the accompanying script - such as the change in explosion size over its lifetime, the degree of variation/bumpiness it has, whether it spawns sub-explosions, and so forth.

## ToDo ##
+ Burn marks on neighboring geometry
+ Residual smoke
+ Fireball glow
