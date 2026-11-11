// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#if !NET9_0_OR_GREATER

namespace System.Runtime.CompilerServices {
	[AttributeUsage (AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	sealed class OverloadResolutionPriorityAttribute : Attribute {
		public OverloadResolutionPriorityAttribute (int priority)
		{
			Priority = priority;
		}

		public int Priority { get; }
	}
}

#endif
