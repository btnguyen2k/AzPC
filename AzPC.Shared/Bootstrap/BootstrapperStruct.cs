﻿using System.Reflection;

namespace AzPC.Shared.Bootstrap;
public readonly struct BootstrapperStruct(Type type,
	MethodInfo? methodConfigureServices = default, MethodInfo? methodConfigureServicesAsync = default,
	MethodInfo? methodConfigureBuilder = default, MethodInfo? methodConfigureBuilderAsync = default,
	MethodInfo? methodInitializeServices = default, MethodInfo? methodInitializeServicesAsync = default,
	MethodInfo? methodDecorateApp = default, MethodInfo? methodDecorateAppAsync = default, int priority = 1000)
{
	public readonly Type type = type;
	public readonly int priority = priority;
	public readonly MethodInfo? methodConfigureServices = methodConfigureServices;
	public readonly MethodInfo? methodConfigureServicesAsync = methodConfigureServicesAsync;
	public readonly MethodInfo? methodConfigureBuilder = methodConfigureBuilder;
	public readonly MethodInfo? methodConfigureBuilderAsync = methodConfigureBuilderAsync;
	public readonly MethodInfo? methodInitializeServices = methodInitializeServices;
	public readonly MethodInfo? methodInitializeServicesAsync = methodInitializeServicesAsync;
	public readonly MethodInfo? methodDecorateApp = methodDecorateApp;
	public readonly MethodInfo? methodDecorateAppAsync = methodDecorateAppAsync;
};
