﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Mixpanel.Exceptions;
using Mixpanel.Misc;

namespace Mixpanel.Builders
{
    internal class TrackBuilder : BuilderBase
    {
        private readonly IDictionary<string, Tuple<object, int>> _mixpanelProps;
        private readonly IDictionary<string, object> _otherProps;
        private static readonly Dictionary<string, string> BindingProps =
            new Dictionary<string, string>
            {
                {"event", MixpanelProperty.Event},

                {"token", MixpanelProperty.Token},

                {"distinct_id", MixpanelProperty.DistinctId},
                {"distinctid", MixpanelProperty.DistinctId},

                {"ip", MixpanelProperty.Ip},

                {"time", MixpanelProperty.Time},
            };

        public TrackBuilder(MixpanelConfig config = null)
            : base(config)
        {
            _mixpanelProps = new Dictionary<string, Tuple<object, int>>();
            _otherProps = new Dictionary<string, object>();
        }

        public void Add(string propertyName, object value, int weight = 1)
        {
            var parsedValue = ValueParser.Parse(value);
            if (!parsedValue.Item2) return;

            string bindingProp;
            if (BindingProps.TryGetValue(propertyName.ToLower(), out bindingProp))
            {
                Tuple<object, int> mpProp;
                if (_mixpanelProps.TryGetValue(bindingProp, out mpProp))
                {
                    if (weight > mpProp.Item2)
                        _mixpanelProps[bindingProp] = Tuple.Create(parsedValue.Item1, weight);
                }
                else
                {
                    _mixpanelProps[bindingProp] = Tuple.Create(parsedValue.Item1, weight);
                }
            }
            else
            {
                _otherProps[propertyName] = parsedValue.Item1;
            }
        }

        public override IDictionary<string, object> Object
        {
            get
            {
                var obj = new Dictionary<string, object>();

                // event
                Tuple<object, int> @event;
                if (!_mixpanelProps.TryGetValue(MixpanelProperty.Event, out @event))
                    throw new MixpanelObjectStructureException("'event' property is not set.");

                if (@event.Item1 == null)
                    throw new MixpanelPropertyNullOrEmptyException("'event' property can't be null.");

                if (!(@event.Item1 is string))
                    throw new MixpanelPropertyWrongTypeException("'event' property should be of type string.");

                var eventS = @event.Item1 as string;
                if (string.IsNullOrWhiteSpace(eventS))
                    throw new MixpanelPropertyNullOrEmptyException("'event' property can't be empty.");
                obj["event"] = eventS;

                var properties = new Dictionary<string, object>();
                obj["properties"] = properties;

                // token
                Tuple<object, int> token;
                if (!_mixpanelProps.TryGetValue(MixpanelProperty.Token, out token))
                    throw new MixpanelObjectStructureException("'token' property is not set.");

                if (token.Item1 == null)
                    throw new MixpanelPropertyNullOrEmptyException("'token' property can't be null.");

                if (!(token.Item1 is string))
                    throw new MixpanelPropertyWrongTypeException("'token' property should be of type string.");

                var tokenS = token.Item1 as string;
                if (string.IsNullOrWhiteSpace(tokenS))
                    throw new MixpanelPropertyNullOrEmptyException("'token' property can't be empty.");
                properties["token"] = tokenS;

                // distinct_id
                Tuple<object, int> distinctId;
                if (_mixpanelProps.TryGetValue(MixpanelProperty.DistinctId, out distinctId)
                    && distinctId.Item1 != null)
                {
                    properties["distinct_id"] = distinctId.Item1.ToString();
                }

                // ip
                Tuple<object, int> ip;
                if (_mixpanelProps.TryGetValue(MixpanelProperty.Ip, out ip) && ip.Item1 != null)
                {
                    properties["ip"] = ip.Item1.ToString();
                }

                // time
                Tuple<object, int> time;
                if (_mixpanelProps.TryGetValue(MixpanelProperty.Time, out time) && time.Item1 != null)
                {
                    DateTime dateTime;
                    if (DateTime.TryParseExact(time.Item1.ToString(), ValueParser.MixpanelDateFormat,
                            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out dateTime))
                    {
                        properties["time"] = dateTime.ToUnixTime();
                    }
                }

                // Other properties
                //TODO: Names changing according to config
                foreach (var otherProp in _otherProps)
                {
                    properties[otherProp.Key] = otherProp.Value;
                }

                return obj;
            }
        }
    }
}