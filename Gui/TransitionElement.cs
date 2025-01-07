using Blockgame_OpenTK.Util;
using OpenTK.Mathematics;
using System;
using System.ComponentModel.DataAnnotations;

namespace Blockgame_OpenTK.Gui
{

    public enum InterpolationMode
    {

        Linear,
        Cosine,
        EaseOutCubic

    }

    public enum PlaybackMode
    {

        Forward,
        Reverse

    }

    internal class TransitionElement
    {

        public GuiElement Target = null;

        private float _time = 0.0f;
        public float Length = 1.0f;
        public bool IsRunning = false;
        public bool ShouldLoop = false;
        public bool IsPingPong = false;
        public PlaybackMode PlaybackMode = PlaybackMode.Forward;
        public InterpolationMode InterpolationMode = InterpolationMode.Linear;

        public Vector2? StartingRelativePosition;
        public Vector2? EndingRelativePosition;
        public Vector2? StartingAbsolutePosition;
        public Vector2? EndingAbsolutePosition;

        public Vector2? StartingRelativeDimensions;
        public Vector2? EndingRelativeDimensions;
        public Vector2? StartingAbsoluteDimensions;
        public Vector2? EndingAbsoluteDimensions;

        public Color4<Rgba>? StartingColor;
        public Color4<Rgba>? EndingColor;

        public TransitionElement(GuiElement target)
        {

            Target = target;
            target.TransitionElement = this;

        }

        public void Play()
        {

            IsRunning = true;

        }

        public void Pause()
        {

            IsRunning = false;

        }

        public void Stop()
        {

            IsRunning = false;
            _time = 0.0f;

        }

        public void Reset()
        {

            _time = 0.0f;

        }

        public void Update()
        {

            if (IsRunning)
            {

                if (IsPingPong)
                {

                    if (_time > Length) PlaybackMode = PlaybackMode.Reverse;
                    if (_time < 0) PlaybackMode = PlaybackMode.Forward;

                }

                if (ShouldLoop)
                {

                    if (PlaybackMode == PlaybackMode.Forward)
                    {

                        if (_time > Length) _time = 0.0f;

                    }
                    if (PlaybackMode == PlaybackMode.Reverse)
                    {

                        if (_time < 0) _time = Length;

                    }

                } else
                {

                    if (PlaybackMode == PlaybackMode.Forward)
                    {

                        if (_time > Length)
                        {

                            _time = Length;
                            IsRunning = false;

                        }

                    }
                    if (PlaybackMode == PlaybackMode.Reverse)
                    {

                        if (_time < 0)
                        {

                            _time = 0;
                            IsRunning = false;

                        }

                    }

                }

                switch (InterpolationMode)
                {

                    case InterpolationMode.Linear:
                        if (StartingRelativePosition != null)
                        {
                            Target.RelativePosition = Vector2.Lerp(StartingRelativePosition ?? Target.RelativePosition, EndingRelativePosition ?? Target.RelativePosition, _time / Length);
                        }
                        if (StartingRelativeDimensions != null)
                        {
                            Target.RelativeDimensions = Vector2.Lerp(StartingRelativeDimensions ?? Target.RelativeDimensions, EndingRelativeDimensions ?? Target.RelativeDimensions, _time / Length);
                        }
                        if (StartingAbsolutePosition != null)
                        {
                            Target.AbsolutePosition = Vector2.Lerp(StartingAbsolutePosition ?? Target.AbsolutePosition, EndingAbsolutePosition ?? Target.AbsolutePosition, _time / Length);
                        }
                        if (StartingAbsoluteDimensions != null)
                        {
                            Target.AbsoluteDimensions = Vector2.Lerp(StartingAbsoluteDimensions ?? Target.AbsoluteDimensions, EndingAbsoluteDimensions ?? Target.AbsoluteDimensions, _time / Length);
                        }
                        if (StartingColor != null)
                        {
                            Target.Color.X = Maths.Lerp(StartingColor?.X ?? Target.Color.X, EndingColor?.X ?? Target.Color.X, _time / Length);
                            Target.Color.Y = Maths.Lerp(StartingColor?.Y ?? Target.Color.Y, EndingColor?.Y ?? Target.Color.Y, _time / Length);
                            Target.Color.Z = Maths.Lerp(StartingColor?.Z ?? Target.Color.Z, EndingColor?.Z ?? Target.Color.Z, _time / Length);
                            Target.Color.W = Maths.Lerp(StartingColor?.W ?? Target.Color.W, EndingColor?.W ?? Target.Color.W, _time / Length);
                        }
                        break;
                    case InterpolationMode.Cosine:
                        if (StartingRelativePosition != null)
                        {
                            Target.RelativePosition = VectorMath.CosLerp(StartingRelativePosition ?? Target.RelativePosition, EndingRelativePosition ?? Target.RelativePosition, _time / Length);
                        }
                        if (StartingRelativeDimensions != null)
                        {
                            Target.RelativeDimensions = VectorMath.CosLerp(StartingRelativeDimensions ?? Target.RelativeDimensions, EndingRelativeDimensions ?? Target.RelativeDimensions, _time / Length);
                        }
                        if (StartingAbsolutePosition != null)
                        {
                            Target.AbsolutePosition = VectorMath.CosLerp(StartingAbsolutePosition ?? Target.AbsolutePosition, EndingAbsolutePosition ?? Target.AbsolutePosition, _time / Length);
                        }
                        if (StartingAbsoluteDimensions != null)
                        {
                            Target.AbsoluteDimensions = VectorMath.CosLerp(StartingAbsoluteDimensions ?? Target.AbsoluteDimensions, EndingAbsoluteDimensions ?? Target.AbsoluteDimensions, _time / Length);
                        }
                        if (StartingColor != null)
                        {
                            Target.Color.X = Maths.CosLerp(StartingColor?.X ?? Target.Color.X, EndingColor?.X ?? Target.Color.X, _time / Length);
                            Target.Color.Y = Maths.CosLerp(StartingColor?.Y ?? Target.Color.Y, EndingColor?.Y ?? Target.Color.Y, _time / Length);
                            Target.Color.Z = Maths.CosLerp(StartingColor?.Z ?? Target.Color.Z, EndingColor?.Z ?? Target.Color.Z, _time / Length);
                            Target.Color.W = Maths.CosLerp(StartingColor?.W ?? Target.Color.W, EndingColor?.W ?? Target.Color.W, _time / Length);
                        }
                        break;
                    case InterpolationMode.EaseOutCubic:
                        if (StartingRelativePosition != null)
                        {
                            Target.RelativePosition = VectorMath.EaseOutCubic(StartingRelativePosition ?? Target.RelativePosition, EndingRelativePosition ?? Target.RelativePosition, _time / Length);
                        }
                        if (StartingRelativeDimensions != null)
                        {
                            Target.RelativeDimensions = VectorMath.EaseOutCubic(StartingRelativeDimensions ?? Target.RelativeDimensions, EndingRelativeDimensions ?? Target.RelativeDimensions, _time / Length);
                        }
                        if (StartingAbsolutePosition != null)
                        {
                            Target.AbsolutePosition = VectorMath.EaseOutCubic(StartingAbsolutePosition ?? Target.AbsolutePosition, EndingAbsolutePosition ?? Target.AbsolutePosition, _time / Length);
                        }
                        if (StartingAbsoluteDimensions != null)
                        {
                            Target.AbsoluteDimensions = VectorMath.EaseOutCubic(StartingAbsoluteDimensions ?? Target.AbsoluteDimensions, EndingAbsoluteDimensions ?? Target.AbsoluteDimensions, _time / Length);
                        }
                        if (StartingColor != null)
                        {
                            Target.Color.X = Maths.Lerp(StartingColor?.X ?? Target.Color.X, EndingColor?.X ?? Target.Color.X, Maths.EaseOutCubic(_time / Length));
                            Target.Color.Y = Maths.Lerp(StartingColor?.Y ?? Target.Color.Y, EndingColor?.Y ?? Target.Color.Y, Maths.EaseOutCubic(_time / Length));
                            Target.Color.Z = Maths.Lerp(StartingColor?.Z ?? Target.Color.Z, EndingColor?.Z ?? Target.Color.Z, Maths.EaseOutCubic(_time / Length));
                            Target.Color.W = Maths.Lerp(StartingColor?.W ?? Target.Color.W, EndingColor?.W ?? Target.Color.W, Maths.EaseOutCubic(_time / Length));
                        }
                        break;

                }

                // _time += (float)GlobalValues.DeltaTime;
                if (PlaybackMode == PlaybackMode.Forward) _time += (float)GlobalValues.DeltaTime;
                if (PlaybackMode == PlaybackMode.Reverse) _time -= (float)GlobalValues.DeltaTime;

            }

        }

    }
}
