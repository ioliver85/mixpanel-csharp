﻿using System.Collections.Generic;

namespace Mixpanel.Builders
{
    /// <summary>
    /// Base class for building mixpanel objects (dictionaries) that will be serialized to json 
    /// and sent to Mixpanel.
    /// </summary>
    internal abstract class BuilderBase
    {
        protected readonly MixpanelConfig Config;
        protected readonly ValueParser ValueParser;

        protected BuilderBase(MixpanelConfig config = null)
        {
            Config = config;
            ValueParser = new ValueParser();
        }

        public abstract IDictionary<string, object> Object { get; } 
    }
}