# gridanimation
For one of my most recent projects I had to create a grid system, where the player had to tap on in order to cast spells. We wanted to make it look as awesome as possible, so I wrote a system where the grid tiles could show a cool ripple effect, and then display an image based on a png.

Something I really liked about this system was how the ripple worked. All I had to do was to start a coroutine and pass a function as parameter. Whenever a tile got selected by the ripple coroutine, the function given as parameter would then decide what happened to the tile, making a very reusable system.
                        
