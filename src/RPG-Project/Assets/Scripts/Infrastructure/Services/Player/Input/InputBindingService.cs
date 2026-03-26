using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Infrastructure.Services.Player.Input
{
    public sealed class InputBindingService : IInputBindingService
    {
        private const string BindingOverridesKey = "input.binding.overrides";

        private readonly global::PlayerInput _playerInput;
        private readonly Dictionary<InputBindingKey, BindingDescriptor> _bindings;

        private InputActionRebindingExtensions.RebindingOperation _rebindOperation;

        public bool IsRebinding => _rebindOperation != null;

        public InputBindingService(global::PlayerInput playerInput)
        {
            _playerInput = playerInput;
            _bindings = new Dictionary<InputBindingKey, BindingDescriptor>
            {
                { InputBindingKey.MoveUp, new BindingDescriptor(_playerInput.Player.Move, 1) },
                { InputBindingKey.MoveDown, new BindingDescriptor(_playerInput.Player.Move, 3) },
                { InputBindingKey.MoveLeft, new BindingDescriptor(_playerInput.Player.Move, 5) },
                { InputBindingKey.MoveRight, new BindingDescriptor(_playerInput.Player.Move, 7) },
                { InputBindingKey.Jump, new BindingDescriptor(_playerInput.Player.Jump, 0) },
                { InputBindingKey.Sprint, new BindingDescriptor(_playerInput.Player.Sprint, 0) },
                { InputBindingKey.SwordAttack, new BindingDescriptor(_playerInput.Player.SwordAttack, 0) },
                { InputBindingKey.MagicAttack, new BindingDescriptor(_playerInput.Player.MagicAttack, 0) },
            };

            LoadOverrides();
        }

        public string GetBindingDisplay(InputBindingKey bindingKey)
        {
            var binding = _bindings[bindingKey];
            var displayName = binding.Action.GetBindingDisplayString(binding.BindingIndex, InputBinding.DisplayStringOptions.DontIncludeInteractions);

            return string.IsNullOrWhiteSpace(displayName) ? "Not Bound" : displayName;
        }

        public bool StartRebind(InputBindingKey bindingKey, Action onComplete = null, Action onCancel = null)
        {
            if (IsRebinding)
            {
                return false;
            }

            var binding = _bindings[bindingKey];
            bool wasEnabled = binding.Action.enabled;

            binding.Action.Disable();

            _rebindOperation = binding.Action
                .PerformInteractiveRebinding(binding.BindingIndex)
                .WithControlsHavingToMatchPath("<Keyboard>")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnCancel(operation => FinishRebind(binding.Action, wasEnabled, operation, onCancel, saveOverrides: false))
                .OnComplete(operation => FinishRebind(binding.Action, wasEnabled, operation, onComplete, saveOverrides: true));

            _rebindOperation.Start();
            return true;
        }

        public void CancelRebind()
        {
            _rebindOperation?.Cancel();
        }

        private void FinishRebind(
            InputAction action,
            bool wasEnabled,
            InputActionRebindingExtensions.RebindingOperation operation,
            Action callback,
            bool saveOverrides)
        {
            operation.Dispose();
            _rebindOperation = null;

            if (wasEnabled)
            {
                action.Enable();
            }

            if (saveOverrides)
            {
                SaveOverrides();
            }

            callback?.Invoke();
        }

        private void LoadOverrides()
        {
            string overridesJson = PlayerPrefs.GetString(BindingOverridesKey, string.Empty);
            if (string.IsNullOrWhiteSpace(overridesJson))
            {
                return;
            }

            _playerInput.asset.LoadBindingOverridesFromJson(overridesJson);
        }

        private void SaveOverrides()
        {
            string overridesJson = _playerInput.asset.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(BindingOverridesKey, overridesJson);
            PlayerPrefs.Save();
        }

        private readonly struct BindingDescriptor
        {
            public readonly InputAction Action;
            public readonly int BindingIndex;

            public BindingDescriptor(InputAction action, int bindingIndex)
            {
                Action = action;
                BindingIndex = bindingIndex;
            }
        }
    }
}
