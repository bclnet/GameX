﻿using Microsoft.Maui;
using System;
using Microsoft.Maui.Platform;
using PlatformView = StereoKit.UIX.Controls.View;
using StereoKit.Maui.Platform;

namespace StereoKit.Maui.Handlers
{
    public partial class SKViewHandler
    {
        static partial void MappingFrame(IViewHandler handler, IView view) => UpdateTransformation(handler, view);

        public static void MapTranslationX(IViewHandler handler, IView view) => UpdateTransformation(handler, view);

        public static void MapTranslationY(IViewHandler handler, IView view) => UpdateTransformation(handler, view);

        public static void MapScale(IViewHandler handler, IView view) => UpdateTransformation(handler, view);

        public static void MapScaleX(IViewHandler handler, IView view) => UpdateTransformation(handler, view);

        public static void MapScaleY(IViewHandler handler, IView view) => UpdateTransformation(handler, view);

        public static void MapRotation(IViewHandler handler, IView view) => UpdateTransformation(handler, view);

        public static void MapRotationX(IViewHandler handler, IView view) => UpdateTransformation(handler, view);

        public static void MapRotationY(IViewHandler handler, IView view) => UpdateTransformation(handler, view);

        public static void MapAnchorX(IViewHandler handler, IView view) => UpdateTransformation(handler, view);

        public static void MapAnchorY(IViewHandler handler, IView view) => UpdateTransformation(handler, view);

        public static void MapContextFlyout(IViewHandler handler, IView view) { }

        public static void MapToolbar(IViewHandler handler, IView view)
        {
            if (handler.VirtualView is not IToolbarElement te || te.Toolbar == null)
                return;

            _ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

            var platformToolbar = te.Toolbar?.ToPlatform(handler.MauiContext);

            //if (handler.PlatformView is IToolbarContainer toolbarContainer)
            //    toolbarContainer.SetToolbar((MauiToolbar)platformToolbar!);
            //else
            //    handler.MauiContext.GetToolbarContainer()?.SetToolbar((MauiToolbar)platformToolbar!);
        }

        internal static void MapToolbar(IElementHandler handler, IToolbarElement toolbarElement)
        {
            if (toolbarElement.Toolbar == null)
                return;

            _ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(handler.MauiContext)} null");

            var platformToolbar = toolbarElement.Toolbar?.ToPlatform(handler.MauiContext);
            //handler.MauiContext.GetToolbarContainer()?.SetToolbar((MauiToolbar)platformToolbar!);
        }

        internal static void UpdateTransformation(IViewHandler handler, IView view) => handler.ToPlatform()?.UpdateTransformation(view);

        protected virtual void OnPlatformViewDeleted() { }

        protected virtual void OnFocused() { }

        protected virtual void OnUnfocused() { }

        protected void OnFocused(object? sender, EventArgs e)
        {
            if (VirtualView != null)
                VirtualView.IsFocused = true;
            OnFocused();
        }

        protected void OnUnfocused(object? sender, EventArgs e)
        {
            if (VirtualView != null)
                VirtualView.IsFocused = false;
            OnUnfocused();
        }

        partial void ConnectingHandler(PlatformView? platformView)
        {
            if (platformView == null)
                return;

            platformView.FocusGained += OnFocused;
            platformView.FocusLost += OnUnfocused;
        }

        partial void DisconnectingHandler(PlatformView platformView)
        {
            if (platformView == null)
                return;
        }

        void OnPlatformViewDeleted(object? sender, EventArgs e) => OnPlatformViewDeleted();
    }
}