﻿using System;
using System.Collections;
using Mixpanel.Misc;
#if !NET35
using System.Collections.Concurrent;
#endif
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;


namespace Mixpanel.Core
{
    internal sealed class PropertiesDigger
    {
        public IDictionary<string, ObjectProperty> Get(object obj)
        {
            var props = new Dictionary<string, ObjectProperty>();
            if (obj == null) return props;

            if (obj is IDictionary<string, object>)
            {
                var dic = obj as IDictionary<string, object>;
                foreach (KeyValuePair<string, object> pair in dic)
                {
                    props[pair.Key] = new ObjectProperty(PropertyNameSource.Default, pair.Value);
                }
            }
            else if (obj is IDictionary)
            {
                var dic = obj as IDictionary;
                foreach (DictionaryEntry entry in dic)
                {
                    var keyS = entry.Key as string;
                    if (keyS == null) continue;
                    props[keyS] = new ObjectProperty(PropertyNameSource.Default, entry.Value);
                }
            }
            else
            {
                foreach (var propertyInfo in GetObjectPropertyInfos(obj.GetType()))
                {
                    props[propertyInfo.PropertyName] = new ObjectProperty(
                        propertyInfo.PropertyNameSource,
                        propertyInfo.PropertyInfo.GetValue(obj, null));
                }
            }

            return props;
        }

#if NET35
        private static readonly ThreadSafeCache<Type, List<ObjectPropertyInfo>> 
            PropertyInfosCache = new ThreadSafeCache<Type, List<ObjectPropertyInfo>>();
#else
        private static readonly ConcurrentDictionary<Type, List<ObjectPropertyInfo>>
            PropertyInfosCache = new ConcurrentDictionary<Type, List<ObjectPropertyInfo>>();
#endif

        private List<ObjectPropertyInfo> GetObjectPropertyInfos(Type type)
        {
            return PropertyInfosCache.GetOrAdd(type, t =>
            {
                bool isDataContract = t.GetCustomAttribute<DataContractAttribute>() != null;
                var infos = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var res = new List<ObjectPropertyInfo>(infos.Length);

                foreach (var info in infos)
                {
                    if (info.GetCustomAttribute<IgnoreDataMemberAttribute>() != null)
                        continue;

                    DataMemberAttribute dataMemberAttr = null;
                    if (isDataContract)
                    {
                        dataMemberAttr = info.GetCustomAttribute<DataMemberAttribute>();
                        if (dataMemberAttr == null)
                            continue;
                    }

                    var mixpanelNameAttr = info.GetCustomAttribute<MixpanelNameAttribute>();
                    if (mixpanelNameAttr != null)
                    {
                        var isMixpanelNameEmpty = mixpanelNameAttr.Name.IsNullOrWhiteSpace();
                        res.Add(new ObjectPropertyInfo(
                            isMixpanelNameEmpty ? info.Name : mixpanelNameAttr.Name,
                            isMixpanelNameEmpty ? PropertyNameSource.Default : PropertyNameSource.MixpanelName,
                            info));
                        continue;
                    }

                    if (dataMemberAttr != null)
                    {
                        var isDataMemberNameEmpty = dataMemberAttr.Name.IsNullOrWhiteSpace();
                        res.Add(new ObjectPropertyInfo(
                            isDataMemberNameEmpty ? info.Name : dataMemberAttr.Name,
                            isDataMemberNameEmpty ? PropertyNameSource.Default : PropertyNameSource.DataMember,
                            info));
                        continue;
                    }

                    res.Add(new ObjectPropertyInfo(info.Name, PropertyNameSource.Default, info));
                }
                return res;
            });
        }
    }
}
