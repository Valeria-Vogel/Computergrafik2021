using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.Scene;
using Fusee.Engine.Core.Effects;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;
using Fusee.Engine.GUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FuseeApp
{
    [FuseeApplication(Name = "Tut08_FirstSteps", Description = "Yet another FUSEE App.")]
    public class Tut08_FirstSteps : RenderCanvas
    {
        private SceneContainer _scene;
        private SceneRendererForward _sceneRenderer;

        private Transform _cubeTransform;
        // Init is called on startup.

        // Referenz von Außen ersellen
        private DefaultSurfaceEffect _cubeShader;
        public override void Init()
        {
            // Set the clear color for the backbuffer to "greenery"
            RC.ClearColor = (float4)ColorUint.Greenery;

            /*     // Set the clear color for the backbuffer to white (100% intensity in all color channels R, G, B, A).
               RC.ClearColor = new float4(1, 1, 1, 1);

                            // Create a scene with a cube
                            // The three components: one Transform, one ShaderEffect (blue material) and the Mesh
                            _cubeTransform = new Transform
                            {
                                Translation = new float3(0, 0, 20),
                                Rotation = new float3(0, 0.3f, 0),
                            };
                            var _cubeShader = MakeEffect.FromDiffuseSpecular((float4)ColorUint.Blue, float4.Zero);
                            var cubeMesh = SimpleMeshes.CreateCuboid(new float3(10, 10, 10));

                            // Assemble the cube node containing the three components
                            var cubeNode = new SceneNode();
                            cubeNode.Components.Add(_cubeTransform);
                            cubeNode.Components.Add(_cubeShader);
                            cubeNode.Components.Add(cubeMesh);

                            // Create the scene containing the cube as the only object
                            _scene = new SceneContainer();
                            _scene.Children.Add(cubeNode);
                            */
            _cubeTransform = new Transform
            {
                Translation = new float3(0, 0, 20),
                Rotation = new float3(0, 0.3f, 0),
            };
            _cubeShader = MakeEffect.FromDiffuseSpecular((float4)ColorUint.Blue, float4.Zero);
            _scene = new SceneContainer
            {
                Children =
                     {
                            new SceneNode
                         {
                           Components =
                            {
                                _cubeTransform,
                                _cubeShader,
                                SimpleMeshes.CreateCuboid(new float3(5, 5, 5))
                             }
                        },
                        new SceneNode
                        {
                            Components =
                            {
                                _cubeTransform, new Transform { 
                                    Translation = new float3(4,3,10),
                                    Rotation = new float3(0.3f, 0.3f,0),
                                    },
                                MakeEffect.FromDiffuseSpecular((float4)ColorUint.Turquoise , float4.Zero),
                                SimpleMeshes.CreateCuboid(new float3(2,2,2))

                            }
                        },
                        new SceneNode
                        {
                            Components =
                            {
                               _cubeTransform, new Transform { 
                                    Translation = new float3(10, -5,-15),
                                    Rotation = new float3(0, 0.03f * Time.DeltaTime, 0)
                                    },
                                MakeEffect.FromDiffuseSpecular((float4)ColorUint.YellowGreen , float4.Zero),
                                SimpleMeshes.CreateCuboid(new float3(3,3,3))

                            }
                        },
                        new SceneNode
                        {
                            Components =
                            {
                               _cubeTransform, new Transform {
                                    Translation = new float3(0,5,-10),
                                    Rotation = new float3(0, 0.3f* Time.DeltaTime,0)
                                    },
                                MakeEffect.FromDiffuseSpecular((float4)ColorUint.Violet , float4.Zero),
                                SimpleMeshes.CreateCuboid(new float3(1,1,1))

                            }
                        },
                         new SceneNode
                        {
                            Components =
                            {
                               _cubeTransform, new Transform {
                                    Translation = new float3(-10,-5,-3),
                                    Rotation = new float3(0,  5 * M.Sin(3 * TimeSinceStart),0)
                                    },
                                MakeEffect.FromDiffuseSpecular((float4)ColorUint.WhiteSmoke , float4.Zero),
                                SimpleMeshes.CreateCuboid(new float3(1,1,1))

                            }
                        }

                    }
            };

            // Create a scene renderer holding the scene above
            _sceneRenderer = new SceneRendererForward(_scene);

        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            SetProjectionAndViewport();

            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            _cubeTransform.Rotation = _cubeTransform.Rotation + new float3(0, 0.003f * Time.DeltaTime, 0);
            //Zeitschwankung durch Frameratenabhängikeit (einsehen?)
            //Diagnostics.Debug("DeltaTime: " + Time.DeltaTime);

            Diagnostics.Debug("Keyboard <-> axis:" + Keyboard.LeftRightAxis);

            _cubeTransform.Rotation = new float3(0, 90 * (3.141592f / 180.0f) * Time.TimeSinceStart, 0);

            Diagnostics.Debug("Keyboard <-> axis: " + Keyboard.LeftRightAxis);
            //Ausschlag des Würfels nach links und rechts
            _cubeTransform.Translation.x = 5 * M.Sin(2 * Time.TimeSinceStart);
            // Zeitausgabe auf der Console seit dem Start


            _cubeShader.SurfaceInput.Albedo = new float4(0.5f + 0.5f * M.Sin(2 * Time.TimeSinceStart), 0, 0, 1);


            // Render the scene on the current render context
            _sceneRenderer.Render(RC);

            // Swap buffers: Show the content 
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)

            //var projection = float4x4.CreatePerspectiveFieldOfView(M.PiOver4, aspectRatio, 1, 20000);
            //RC.Projection = projection;

            Present();
        }
        public void SetProjectionAndViewport()
        {
            // Set the rendering area to the entire window size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width / (float)Height;

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            var projection = float4x4.CreatePerspectiveFieldOfView(M.PiOver4, aspectRatio, 1, 20000);
            RC.Projection = projection;
        }

    }
}