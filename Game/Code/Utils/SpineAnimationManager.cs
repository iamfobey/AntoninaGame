using Godot;
using Godot.Collections;

namespace Game.Utils
{
    public partial class SpineAnimationManager : Node2D
    {
        #region PUBLIC METHODS
        public override void _EnterTreeGame()
        {
            base._EnterTreeGame();

            SpineAnimationState = SpineSprite.GetAnimationState();
        }

        public SpineTrackEntry SetAnimation(StringName animationName, bool loop = true, int trackId = 0)
        {
            if (_currentAnimation == animationName || animationName == "") return null;

            _currentAnimation = animationName;

            return SpineAnimationState.SetAnimation(animationName, loop, trackId);
        }

        public SpineTrackEntry SetAnimationByDirection(StringName animationName, Vector2 direction, bool loop = true, int trackId = 0)
        {
            if (direction == Vector2.Zero)
            {
                StringName forward = animationName + "_" + "Forward";
                if (AnimationPlayer != null && AnimationPlayer.HasAnimation(forward))
                {
                    if (AnimationPlayer.IsPlaying())
                        AnimationPlayer.Stop();
                    AnimationPlayer.Play(forward);
                }

                return SetAnimation(animationName + AnimationNameSeparator + AnimationSides[animationName]["Forward"], loop, trackId);
            }

            float angle = Mathf.RadToDeg(direction.Angle());

            string sideKey = angle switch
            {
                >= -45 and <= 45 => "Right",
                > 45 and < 135 => "Forward",
                >= 135 or <= -135 => "Left",
                _ => "Backward"
            };

            StringName animationPlayerName = animationName + "_" + sideKey;
            if (AnimationPlayer != null && AnimationPlayer.HasAnimation(animationPlayerName))
            {
                if (AnimationPlayer.IsPlaying())
                    AnimationPlayer.Stop();
                AnimationPlayer.Play(animationPlayerName);
            }

            return SetAnimation(animationName + AnimationNameSeparator + AnimationSides[animationName][sideKey], loop, trackId);
        }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Node Refs")]
        [ExportGroup("Base")]
        [Export]
        public AnimationPlayer AnimationPlayer;

        [ExportCategory("Parameters")]
        [ExportGroup("Logic")]
        [Export]
        public Dictionary<StringName, Dictionary<StringName, StringName>> AnimationSides = new();
        [Export]
        public SpineSprite SpineSprite;
        [Export]
        public string AnimationNameSeparator = "/";
        #endregion

        #region PUBLIC FIELDS
        public SpineAnimationState SpineAnimationState;
        #endregion

        #region PRIVATE FIELDS
        private StringName _currentAnimation = "";
        #endregion
    }
}