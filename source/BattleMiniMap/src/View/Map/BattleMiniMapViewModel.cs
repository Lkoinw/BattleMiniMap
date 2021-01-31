﻿using System;
using BattleMiniMap.Config;
using BattleMiniMap.View.AgentMarker;
using BattleMiniMap.View.CameraMarker;
using BattleMiniMap.View.MapTerrain;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screen;

namespace BattleMiniMap.View.Map
{
    public class BattleMiniMapViewModel : ViewModel
    {
        private float _alphaFactor;
        private bool _isEnabled;

        [DataSourceProperty]
        public float AlphaFactor
        {
            get => _alphaFactor;
            set
            {
                if (Math.Abs(_alphaFactor - value) < 0.01f)
                    return;
                _alphaFactor = value;
                OnPropertyChanged(nameof(AlphaFactor));
            }
        }

        [DataSourceProperty]
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value)
                    return;
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        public CameraMarkerViewModel CameraMarkerLeft { get; }
        public CameraMarkerViewModel CameraMarkerRight { get; }

        public MBBindingList<AgentMarkerViewModel> AgentMarkerViewModels { get; }
        public MBBindingList<AgentMarkerViewModel> DeadAgentMarkerViewModels { get; }

        public BattleMiniMapViewModel(MissionScreen missionScreen)
        {
            CameraMarkerLeft = new CameraMarkerViewModel(missionScreen, CameraMarkerSide.Left);
            CameraMarkerRight = new CameraMarkerViewModel(missionScreen, CameraMarkerSide.Right);
            AgentMarkerViewModels = new MBBindingList<AgentMarkerViewModel>();
            DeadAgentMarkerViewModels = new MBBindingList<AgentMarkerViewModel>();
        }

        public void UpdateEnabled(float dt, bool isEnabled)
        {
            bool isFadingCompleted = MiniMap.IsFadingCompleted();
            if (IsEnabled)
            {
                if (isEnabled)
                {
                    if (MiniMap.IsFadingOut())
                    {
                        MiniMap.SetFadeIn();
                    }
                }
                else
                {
                    if (isFadingCompleted)
                    {
                        IsEnabled = false;
                    }
                    else if (!MiniMap.IsFadingOut())
                    {
                        MiniMap.SetFadeOut();
                    }
                }
            }
            else
            {
                if (isEnabled)
                {
                    IsEnabled = true;
                    MiniMap.SetFadeIn();
                }
            }

            MiniMap.UpdateFading(dt);
            AlphaFactor = BattleMiniMapConfig.Get().BackgroundOpacity * MiniMap.FadeInOutAlphaFactor;
        }

        public void UpdateData()
        {
            UpdateAgentMarkers();
            if (!IsEnabled)
                return;
            CameraMarkerLeft.Update();
            CameraMarkerRight.Update();
        }

        public void AddAgent(Agent agent)
        {
            if (agent.IsActive())
            {
                AgentMarkerViewModels.Add(new AgentMarkerViewModel(agent));
            }
            else
            {
                DeadAgentMarkerViewModels.Add(new AgentMarkerViewModel(agent));
            }
        }

        private void UpdateAgentMarkers()
        {
            int count = AgentMarkerViewModels.Count;
            int lastOne = count - 1;
            for (int i = 0; i <= lastOne;)
            {
                var current = AgentMarkerViewModels[i];
                current.Update();
                if (current.AgentMarkerType == AgentMarkerType.Dead)
                {
                    DeadAgentMarkerViewModels.Add(new AgentMarkerViewModel(current));
                    if (i < lastOne)
                    {
                        current.MoveFrom(AgentMarkerViewModels[lastOne]);
                    }
                    --lastOne;
                }
                else
                {
                    ++i;
                }
            }

            if (lastOne < count - 1)
            {
                for (int i = count - 1; i > lastOne ; i--)
                {
                    AgentMarkerViewModels.RemoveAt(i);
                }
            }
        }
    }
}