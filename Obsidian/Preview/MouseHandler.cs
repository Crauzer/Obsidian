﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace Obsidian.Preview
{
    class MouseHandler
    {
        private Vector3D _center;
        private bool _centered; // Have we already determined the rotation center?
        // The state of the trackball
        private bool _enabled;
        private Point _point; // Initial point of drag
        private bool _rotating;
        private Quaternion _rotation;
        private Quaternion _rotationDelta; // Change to rotation because of this drag
        private double _scale;
        private double _scaleDelta; // Change to scale because of this drag
        // The state of the current drag
        private bool _scaling; // Are we scaling?  NOTE otherwise we're rotating
        private List<Viewport3D> _slaves;
        private Vector3D _translate;
        private Vector3D _translateDelta;

        public MouseHandler()
        {
            Reset();
        }

        public List<Viewport3D> Slaves => _slaves ?? (_slaves = new List<Viewport3D>());

        public bool Enabled
        {
            get { return _enabled && (_slaves != null) && (_slaves.Count > 0); }
            set { _enabled = value; }
        }

        public void Attach(FrameworkElement element)
        {
            element.MouseMove += MouseMoveHandler;
            element.MouseRightButtonDown += MouseDownHandler;
            element.MouseRightButtonUp += MouseUpHandler;
            element.MouseWheel += OnMouseWheel;
        }

        public void Detach(FrameworkElement element)
        {
            element.MouseMove -= MouseMoveHandler;
            element.MouseRightButtonDown -= MouseDownHandler;
            element.MouseRightButtonUp -= MouseUpHandler;
            element.MouseWheel -= OnMouseWheel;
        }

        private void UpdateSlaves(Quaternion q, double s, Vector3D t)
        {


            if (_slaves != null)
            {
                foreach (var i in _slaves)
                {
                    var mv = i.Children[0] as ModelVisual3D;
                    var t3Dg = mv.Transform as Transform3DGroup;

                    var groupScaleTransform = t3Dg.Children[0] as ScaleTransform3D;
                    var groupRotateTransform = t3Dg.Children[1] as RotateTransform3D;
                    var groupTranslateTransform = t3Dg.Children[2] as TranslateTransform3D;

                    groupScaleTransform.ScaleX = s;
                    groupScaleTransform.ScaleY = s;
                    groupScaleTransform.ScaleZ = s;
                    groupRotateTransform.Rotation = new AxisAngleRotation3D(q.Axis, q.Angle);
                    groupTranslateTransform.OffsetX = t.X;
                    groupTranslateTransform.OffsetY = t.Y;
                    groupTranslateTransform.OffsetZ = t.Z;

             
                }
            }
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
           
            if (!Enabled) return;
            e.Handled = true;

            var el = (UIElement)sender;

            if (el.IsMouseCaptured)
            {
                var delta = _point - e.MouseDevice.GetPosition(el);
                var t = new Vector3D();

                delta /= 2;
                var q = _rotation;

                if (_rotating)
                {
  
                    var mouse = new Vector3D(delta.X, -delta.Y, 0);
                    var axis = Vector3D.CrossProduct(mouse, new Vector3D(0, 0, 1));
                    var len = axis.Length;
                    if (len < 0.00001 || _scaling)
                        _rotationDelta = new Quaternion(new Vector3D(0, 0, 1), 0);
                    else
                        _rotationDelta = new Quaternion(axis, len);

                    q = _rotationDelta * _rotation;
                }
                else
                {
                    delta /= 20;
                    _translateDelta.X = delta.X * -1;
                    _translateDelta.Y = delta.Y;
                }

                t = _translate + _translateDelta;

                UpdateSlaves(q, _scale * _scaleDelta, t);
            }
        }

        private void MouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (!Enabled) return;
            e.Handled = true;


            if (Keyboard.IsKeyDown(Key.F1))
            {
                Reset();
                return;
            }

            var el = (UIElement)sender;
            _point = e.MouseDevice.GetPosition(el);
            if (!_centered)
            {
                var camera = (ProjectionCamera)_slaves[0].Camera;
                _center = camera.LookDirection;
                _centered = true;
            }

            _scaling = (e.MiddleButton == MouseButtonState.Pressed);

            _rotating = Keyboard.IsKeyDown(Key.Space) == false;

            el.CaptureMouse();
        }

        private void MouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (!_enabled) return;
            e.Handled = true;
            if (_rotating)
                _rotation = _rotationDelta * _rotation;
            else
            {
                _translate += _translateDelta;
                _translateDelta.X = 0;
                _translateDelta.Y = 0;
            }

            var el = (UIElement)sender;
            el.ReleaseMouseCapture();
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            _scaleDelta += e.Delta / (double)1000;
            var q = _rotation;

            UpdateSlaves(q, _scale * _scaleDelta, _translate);
        }

        public void Reset()
        {
            _rotation = new Quaternion(0, 0, 0, 1);
            _scale = 1;
            _translate.X = 0;
            _translate.Y = 0;
            _translate.Z = 0;
            _translateDelta.X = 0;
            _translateDelta.Y = 0;
            _translateDelta.Z = 0;
            _rotationDelta = Quaternion.Identity;
            _scaleDelta = 1;
            UpdateSlaves(_rotation, _scale, _translate);
        }
    }
}

