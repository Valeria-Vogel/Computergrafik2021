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
    [FuseeApplication(Name = "Tut11_AssetsPicking", Description = "Yet another FUSEE App.")]
    public class Tut11_AssetsPicking : RenderCanvas
    {
        private SceneContainer _scene;
        private SceneRendererForward _sceneRenderer;
        private Transform _rightRearWheelTransform, _leftRearWheelTransform, _rightMiddleWheelTransform, _leftMiddleWheelTransform, _rightFrontWheelTransform,
         _leftFrontWheelTransform, _baseArmTransform, _upperArmTransform, _foreArmTransform, _firstFingerTransform, _secondFingerTransform;
        private SurfaceEffect _rightRearWheelEffect, _leftRearWheelEffect, _rightMiddlelWheelEffect, _leftMiddleWheelEffect, _rightFrontWheelEffect,
         _leftFrontWheelEffect, _baseArmEffect, _upperArmEffect, _foreArmEffect, _firstFingerEffect, _secondFingerEffect;
        private ScenePicker _scenePicker;
        private Transform _baseTransform;
        private PickResult _currentPick;
        private float4 _oldColor;

        private static float _angelHorz = M.PiOver4, _angelVert;
        private static float _angelVelHorz, _angelVelVert;
        private const float RotationSpeed = 7;

        private const float Damping = 0.8f;
        private float _angel = 0.5f;
        private Boolean spacePressed = false;
        private Boolean opening = true;


        SceneContainer CreateScene()
        {
            // Initialize transform components that need to be changed inside "RenderAFrame"
            _baseTransform = new Transform
            {
                Rotation = new float3(0, 0, 0),
                Scale = new float3(1, 1, 1),
                Translation = new float3(0, 0, 0)
            };

            // Setup the scene graph
            return new SceneContainer
            {
                Children = new List<SceneNode>
                {
                    new SceneNode
                    {
                        Components = new List<SceneComponent>
                        {
                            // TRANSFROM COMPONENT
                            _baseTransform,

                            // SHADER EFFECT COMPONENT
                            SimpleMeshes.MakeMaterial((float4) ColorUint.LightGrey),

                            // MESH COMPONENT
                            // SimpleAssetsPickinges.CreateCuboid(new float3(10, 10, 10))
                            SimpleMeshes.CreateCuboid(new float3(10, 10, 10))
                        }
                    },
                }
            };
        }

        // Init is called on startup. 
        public override void Init()
        {
            RC.ClearColor = new float4(0.8f, 0.9f, 0.7f, 1);

            _scene = AssetStorage.Get<SceneContainer>("CubeCar_new.fus");

            _rightRearWheelTransform = _scene.Children.FindNodes(node => node.Name == "RightRearWheel")?.FirstOrDefault()?.GetTransform();
            _leftRearWheelTransform = _scene.Children.FindNodes(node => node.Name == "LeftRearWheel")?.FirstOrDefault()?.GetTransform();
            _rightMiddleWheelTransform = _scene.Children.FindNodes(node => node.Name == "RightMiddleWheel")?.FirstOrDefault()?.GetTransform();
            _leftMiddleWheelTransform = _scene.Children.FindNodes(node => node.Name == "LeftMiddleWheel")?.FirstOrDefault()?.GetTransform();
            _rightFrontWheelTransform = _scene.Children.FindNodes(node => node.Name == "RightFrontWheel")?.FirstOrDefault()?.GetTransform();
            _leftFrontWheelTransform = _scene.Children.FindNodes(node => node.Name == "LeftFrontWheel")?.FirstOrDefault()?.GetTransform();
            _baseArmTransform = _scene.Children.FindNodes(node => node.Name == "BaseArm")?.FirstOrDefault()?.GetTransform();
            _upperArmTransform = _scene.Children.FindNodes(node => node.Name == "UpperArm")?.FirstOrDefault()?.GetTransform();
            _foreArmTransform = _scene.Children.FindNodes(node => node.Name == "ForeArm")?.FirstOrDefault()?.GetTransform();
            _firstFingerTransform = _scene.Children.FindNodes(node => node.Name == "FirstFinger")?.FirstOrDefault()?.GetTransform();
            _secondFingerTransform = _scene.Children.FindNodes(node => node.Name == "SecondFinger")?.FirstOrDefault()?.GetTransform();

            _rightRearWheelEffect = _scene.Children.FindNodes(node => node.Name == "RightRearWheel")?.FirstOrDefault()?.GetComponent<SurfaceEffect>();
            _leftRearWheelEffect = _scene.Children.FindNodes(node => node.Name == "LeftRearWheel")?.FirstOrDefault()?.GetComponent<SurfaceEffect>();
            _rightMiddlelWheelEffect = _scene.Children.FindNodes(node => node.Name == "RightMiddlelWheel")?.FirstOrDefault()?.GetComponent<SurfaceEffect>();
            _leftMiddleWheelEffect = _scene.Children.FindNodes(node => node.Name == "LeftMiddleWheel")?.FirstOrDefault()?.GetComponent<SurfaceEffect>();
            _rightFrontWheelEffect = _scene.Children.FindNodes(node => node.Name == "RightFrontWheel")?.FirstOrDefault()?.GetComponent<SurfaceEffect>();
            _leftFrontWheelEffect = _scene.Children.FindNodes(node => node.Name == "LeftFrontWheel")?.FirstOrDefault()?.GetComponent<SurfaceEffect>();
            _baseArmEffect = _scene.Children.FindNodes(node => node.Name == "BaseArm")?.FirstOrDefault()?.GetComponent<SurfaceEffect>();
            _upperArmEffect = _scene.Children.FindNodes(node => node.Name == "UpperArm")?.FirstOrDefault()?.GetComponent<SurfaceEffect>();
            _foreArmEffect = _scene.Children.FindNodes(node => node.Name == "ForeArm")?.FirstOrDefault()?.GetComponent<SurfaceEffect>();
            _firstFingerEffect = _scene.Children.FindNodes(node => node.Name == "FirstFinger")?.FirstOrDefault()?.GetComponent<SurfaceEffect>();
            _secondFingerEffect = _scene.Children.FindNodes(node => node.Name == "SecondFinger")?.FirstOrDefault()?.GetComponent<SurfaceEffect>();

            // Create a scene renderer holding the scene above
            _sceneRenderer = new SceneRendererForward(_scene);
            _scenePicker = new ScenePicker(_scene);
        }

        // RenderAFrame is called once a frame
        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            SetProjectionAndViewport();

            _rightRearWheelTransform.Rotation = new float3(-M.MinAngle(TimeSinceStart), 0, 0);
            _leftRearWheelTransform.Rotation = new float3(-M.MinAngle(TimeSinceStart), 0, 0);
            _rightMiddleWheelTransform.Rotation = new float3(-M.MinAngle(TimeSinceStart), 0, 0);
            _leftMiddleWheelTransform.Rotation = new float3(-M.MinAngle(TimeSinceStart), 0, 0);
            _rightFrontWheelTransform.Rotation = new float3(-M.MinAngle(TimeSinceStart), 0, 0);
            _leftFrontWheelTransform.Rotation = new float3(-M.MinAngle(TimeSinceStart), 0, 0);

            _baseArmTransform.Rotation.y += Keyboard.LeftRightAxis * DeltaTime;
            _upperArmTransform.Translation += Keyboard.UpDownAxis * DeltaTime;
            _foreArmTransform.Translation += Keyboard.WSAxis * DeltaTime;

            _firstFingerTransform.Rotation.z= -_angel;
            _secondFingerTransform.Rotation.z = _angel;

            if (opening)
            {
                if (_angel < 0.5f)
                {
                    _angel += 0.5f * DeltaTime;
                }
            }
            else
            {
                if (_angel > -0.5f)
                {
                    _angel -= 0.5f * DeltaTime;
                }
            }

            if (Keyboard.GetKey(KeyCodes.Space))
            {
                if (!spacePressed)
                {
                    opening = !opening;
                }
                spacePressed = true;
            }
            else
            {
                spacePressed = false;
            }




            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            // Setup the camera 
            RC.View = float4x4.CreateTranslation(0, 0, 40) * float4x4.CreateRotationX(-(float)Math.Atan(15.0 / 40.0));

            if (Mouse.LeftButton)
            {
                float2 pickPosClip = Mouse.Position * new float2(2.0f / Width, -2.0f / Height) + new float2(-1, 1);

                PickResult newPick = _scenePicker.Pick(RC, pickPosClip).OrderBy(pr => pr.ClipPos.z).FirstOrDefault();

                if (newPick?.Node != _currentPick?.Node)
                {
                    if (_currentPick != null)
                    {
                        var ef = _currentPick.Node.GetComponent<DefaultSurfaceEffect>();
                        ef.SurfaceInput.Albedo = _oldColor;
                    }
                    if (newPick != null)
                    {
                        var ef = newPick.Node.GetComponent<SurfaceEffect>();
                        _oldColor = ef.SurfaceInput.Albedo;
                        ef.SurfaceInput.Albedo = (float4)ColorUint.OrangeRed;
                    }
                    _currentPick = newPick;
                }
            }

            //Create Moving possibility with mouse to look around
            if (Mouse.MiddleButton)
            {
                _angelVelHorz = -RotationSpeed * Mouse.XVel * DeltaTime * 0.0005f;
                _angelVelVert = -RotationSpeed * Mouse.YVel * DeltaTime * 0.0005f;
            }
            else if (Touch.GetTouchActive(TouchPoints.Touchpoint_0))
            {

                var touchVel = Touch.GetVelocity(TouchPoints.Touchpoint_0);
                _angelVelHorz = -RotationSpeed * touchVel.x * DeltaTime * 0.0005f;
                _angelVelVert = -RotationSpeed * touchVel.y * DeltaTime * 0.0005f;
            }

            _angelHorz += _angelVelHorz;
            _angelVert += _angelVelVert;

            var mtxRot = float4x4.CreateRotationX(_angelVert) * float4x4.CreateRotationY(_angelHorz);
            var mtxCam = float4x4.LookAt(0, 10, -30, 0, 1, 0, 0, 10, 0);
            RC.View = mtxCam * mtxRot;

            // Render the scene on the current render context
            _sceneRenderer.Render(RC);

            if (_currentPick != null)
            {
                _currentPick.Node.GetTransform().Rotation.y += Keyboard.UpDownAxis * DeltaTime;
            }

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
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