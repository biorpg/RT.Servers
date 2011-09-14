﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RT.Servers
{
    /// <summary>
    /// Encapsulates the various ways in which a URL can map to a request handler. Add instances of this class to <see cref="HttpServer.RequestHandlerHooks"/>
    /// to hook a handler to a specific <see cref="HttpServer"/> instance. This class is immutable.
    /// </summary>
    public sealed class HttpRequestHandlerHook : IEquatable<HttpRequestHandlerHook>, IComparable<HttpRequestHandlerHook>
    {
        /// <summary>Gets a value indicating what domain name the handler applies to. Returns null if it applies to all domains.</summary>
        /// <seealso cref="SpecificDomain"/>
        public string Domain { get; private set; }

        /// <summary>Gets a value indicating what port the handler applies to. Returns null if it applies to all ports.</summary>
        public int? Port { get; private set; }

        /// <summary>Gets a value indicating what URL path the handler applies to. Returns null if it applies to all paths.</summary>
        /// <seealso cref="SpecificPath"/>
        public string Path { get; private set; }

        /// <summary>Gets a value indicating whether the handler applies to all subdomains of the domain specified by
        /// <see cref="Domain"/> (false) or the specific domain only (true).</summary>
        public bool SpecificDomain { get; private set; }

        /// <summary>Gets a value indicating whether the handler applies to all subpaths of the path specified by
        /// <see cref="Path"/> (false) or to the specific path only (true).</summary>
        public bool SpecificPath { get; private set; }

        /// <summary>Gets the request handler for this hook.</summary>
        public Func<HttpRequest, HttpResponse> Handler { get; private set; }

        /// <summary>Initialises a new <see cref="HttpRequestHandlerHook"/>.</summary>
        /// <param name="domain">If null, the handler applies to all domain names. Otherwise, the handler applies to this
        /// domain and all subdomains or to this domain only, depending on the value of <paramref name="specificDomain"/>.</param>
        /// <param name="port">If null, the handler applies to all ports; otherwise to the specified port only.</param>
        /// <param name="path">If null, the handler applies to all URL paths. Otherwise, the handler applies to this
        /// path and all subpaths or to this path only, depending on the value of <paramref name="specificPath"/>.</param>
        /// <param name="specificDomain">If false, the handler applies to all subdomains of the domain specified by
        /// <paramref name="domain"/>. Otherwise it applies to the specific domain only.</param>
        /// <param name="specificPath">If false, the handler applies to all subpaths of the path specified by
        /// <paramref name="path"/>. Otherwise it applies to the specific path only.</param>
        /// <param name="handler">The request handler to hook.</param>
        public HttpRequestHandlerHook(Func<HttpRequest, HttpResponse> handler, string domain = null, int? port = null, string path = null, bool specificDomain = false, bool specificPath = false)
        {
            if (domain == null && specificDomain)
                throw new ArgumentException("If the specificDomain parameter is set to true, a non-null domain must be specified using the domain parameter.");
            if (domain != null && !Regex.IsMatch(domain, @"^[-.a-z0-9]+$"))
                throw new ArgumentException("The domain specified by the domain parameter must not contain any characters other than lower-case a-z, 0-9, hypen (-) or period (.).");
            if (domain != null && (domain.Contains(".-") || domain.Contains("-.") || domain.StartsWith("-") || domain.EndsWith("-")))
                throw new ArgumentException("The domain specified by the domain parameter must not contain a domain name beginning or ending with a hyphen (-).");
            if (domain != null && !specificDomain && domain.StartsWith("."))
                throw new ArgumentException(@"If the specificDomain parameter is set to false or not specified, the domain specified by the domain parameter must not begin with a period (.). It will, however, be treated as a domain. For example, if you specify the domain ""cream.net"", only domains ending in "".cream.net"" and the domain ""cream.net"" itself are matched. The domain ""scream.net"" would not be considered a match. If you wish to hook the handler to every domain, set domain to null.");
            if (domain != null && (domain.StartsWith(".") || domain.EndsWith(".")))
                throw new ArgumentException(@"The domain specified by the domain parameter must not begin or end with a period (.).");

            if (path == null && specificPath)
                throw new ArgumentException("If the specificPath parameter is set to true, a non-null path must be specified using the path parameter.");
            if (path != null && !Regex.IsMatch(path, @"^/[-;/:@=&$_\.\+!*'\(\),a-zA-Z0-9]*$"))
                throw new ArgumentException("The path specified by the path parameter must not contain any characters that are invalid in URLs, or the question mark (?) character, and it must begin with a slash (/).");
            if (path != null && !specificPath && path.EndsWith("/"))
                throw new ArgumentException(@"If the specificPath parameter is set to false or not specified, the path specified by the path parameter must not end with a slash (/). It will, however, be treated as a directory. For example, if you specify the path ""/files"", only URLs beginning with ""/files/"" and the URL ""/files"" itself are matched. The URL ""/fileshare"" would not be considered a match. If you wish to hook the handler to the root directory of the domain, set path to null.");

            if (handler == null)
                throw new ArgumentException("The handler specified by the handler parameter cannot be null.");
            if (path != null && !path.StartsWith("/"))
                throw new ArgumentException("A path specified by the path parameter must begin with the slash character (\"/\").");
            if (port != null && (port.Value < 1 || port.Value > 65535))
                throw new ArgumentException("The port parameter must contain an integer in the range 1 to 65535 or null.");

            Domain = domain;
            Port = port;
            Path = path;
            SpecificDomain = specificDomain;
            SpecificPath = specificPath;
            Handler = handler;
        }

        /// <summary>Compares this hook to another one, and returns a value indicating which hook should be checked for a match first.</summary>
        public int CompareTo(HttpRequestHandlerHook other)
        {
            int result;
            // "any port" handlers match last; other port handlers match in numeric port order
            result = (this.Port ?? int.MaxValue).CompareTo(other.Port ?? int.MaxValue);
            if (result != 0) return result;
            // specific single domains match first
            result = (this.SpecificDomain ? 0 : 1).CompareTo(other.SpecificDomain ? 0 : 1);
            if (result != 0) return result;
            // match more specified domains before less specified ones (e.g. blah.thingy.stuff matches before thingy.stuff)
            // match catch-all domain last
            if (this.Domain != null || other.Domain != null)
            {
                if (this.Domain != null && other.Domain == null)
                    return -1;
                else if (this.Domain == null && other.Domain != null)
                    return 1;
                else if (this.Domain != other.Domain)
                {
                    result = -this.Domain.Length.CompareTo(other.Domain.Length);
                    if (result != 0) return result;
                }
            }
            // specific single paths match first
            result = -this.SpecificPath.CompareTo(other.SpecificPath);
            if (result != 0) return result;
            // match more specific paths before less specific ones (e.g. /blah/thingy/stuff matches before /blah/thingy)
            // match catch-all path last
            if (this.Path == other.Path)  // includes the case when both are null
                return 0;
            if (this.Path != null && other.Path == null)
                return -1;
            if (this.Path == null && other.Path != null)
                return 1;

            return -this.Path.Length.CompareTo(other.Path.Length);
        }

        /// <summary>Compares hooks for equality. Two hooks are equal if their match specification is the same; the handler is ignored.</summary>
        public bool Equals(HttpRequestHandlerHook other)
        {
            if (this.Path != other.Path) return false;
            if (this.SpecificPath != other.SpecificPath) return false;
            if (this.Domain != null || other.Domain != null) // string.Equals of two nulls is, unfortunately, false.
                if (string.Equals(this.Domain, other.Domain, StringComparison.OrdinalIgnoreCase)) return false;
            if (this.SpecificDomain != other.SpecificDomain) return false;
            if (this.Port != other.Port) return false;
            return true;
        }

        /// <summary>Compares hooks for equality. Two hooks are equal if their match specification is the same; the handler is ignored.</summary>
        public override bool Equals(object obj)
        {
            var other = obj as HttpRequestHandlerHook;
            if (other == null)
                return false; // could be an actual null or just a different type
            return this.Equals(other);
        }

        /// <summary>Computes a hash code suitable as a heuristic for the kind of equality defined by <see cref="Equals(object)"/>.</summary>
        public override int GetHashCode()
        {
            return unchecked(
                (Path == null ? -47 : Path.GetHashCode()) * 1669 +
                (Domain == null ? -53 : Domain.GetHashCode()) * 2089 +
                (Port == null ? -59 : Port.Value) * 2819 +
                (SpecificPath ? 7919 : 5237) +
                (SpecificDomain ? 6709 : 4079)
            );
        }

        /// <summary>Returns a debugging-friendly representation of this hook's match specification.</summary>
        public override string ToString()
        {
            return (this.Domain == null ? "http://*" : ((this.SpecificDomain ? "http://" : "http://*.") + this.Domain))
                + (this.Port == null ? "" : (":" + this.Port))
                + (this.Path == null ? "/*" : (this.Path + (this.SpecificPath ? "" : "/*")));
        }
    }

    /// <summary>
    /// Maintains a collection of <see cref="HttpRequestHandlerHook"/> objects, making sure that they are always enumerated
    /// in a sensible order, ensuring that there are no duplicate hooks added in error, and synchronizing all operations except for
    /// enumeration.
    /// </summary>
    public sealed class HttpRequestHandlerHookCollection : ICollection<HttpRequestHandlerHook>
    {
        private List<HttpRequestHandlerHook> _hooks = new List<HttpRequestHandlerHook>();

        /// <summary>Gets a value indicating the number of hooks in this collection.</summary>
        public int Count
        {
            get
            {
                lock (this)
                    return _hooks.Count;
            }
        }

        /// <summary>Determines whether a hook with the same match specification is present in this collection.</summary>
        public bool Contains(HttpRequestHandlerHook item)
        {
            lock (this)
                return _hooks.BinarySearch(item) >= 0;
        }

        /// <summary>Enumerates the hooks. To maintain thread safety, you must hold a lock on the instance until the enumeration is finished.</summary>
        public IEnumerator<HttpRequestHandlerHook> GetEnumerator() { return _hooks.GetEnumerator(); }

        /// <summary>Enumerates the hooks. To maintain thread safety, you must hold a lock on the instance until the enumeration is finished.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }

        /// <summary>Throws a "not implemented" exception.</summary>
        public void CopyTo(HttpRequestHandlerHook[] array, int arrayIndex) { throw new NotImplementedException(); }

        /// <summary>Returns false.</summary>
        public bool IsReadOnly { get { return false; } }

        /// <summary>Removes all hooks from this collection.</summary>
        public void Clear()
        {
            lock (this)
                _hooks.Clear();
        }

        /// <summary>Adds the specified hook to this collection.</summary>
        public void Add(HttpRequestHandlerHook item)
        {
            lock (this)
            {
                int i = _hooks.BinarySearch(item);
                if (i >= 0)
                {
                    // Need to check for duplicates both up and down from here, because CompareTo == 0 doesn't imply equality.
                    if (item.Equals(_hooks[i]))
                        throw newDuplicateHookException(item);
                    for (int k = i - 1; k >= 0 && _hooks[k].CompareTo(item) == 0; k--)
                        if (item.Equals(_hooks[k]))
                            throw newDuplicateHookException(item);
                    for (i++; i < _hooks.Count && _hooks[i].CompareTo(item) == 0; i++)
                        if (item.Equals(_hooks[i]))
                            throw newDuplicateHookException(item);
                }
                else
                    i = ~i;
                _hooks.Insert(i, item);
            }
        }

        /// <summary>Efficiently adds multiple hooks to this collection. Use this method whenever adding multiple hooks at once.</summary>
        public void AddRange(IEnumerable<HttpRequestHandlerHook> items)
        {
            lock (this)
            {
                var hooks = _hooks.Concat(items).ToList();
                if (hooks.Count == _hooks.Count)
                    return;
                hooks.Sort();
                var curEqual = 0;
                for (int i = 1; i < hooks.Count; i++)
                {
                    if (hooks[i].CompareTo(hooks[curEqual]) != 0)
                        curEqual = i;
                    for (int j = curEqual; j < i; j++)
                        if (hooks[i].Equals(hooks[j]))
                            throw newDuplicateHookException(hooks[i]);
                }
                _hooks = hooks;
            }
        }

        private Exception newDuplicateHookException(HttpRequestHandlerHook hook)
        {
            return new InvalidOperationException("There is already a request handler hook with the same match specification; the new one would always be ignored. Hook: " + hook.ToString());
        }

        /// <summary>Removes a hook with the given match specification.</summary>
        public bool Remove(HttpRequestHandlerHook item)
        {
            lock (this)
                return _hooks.Remove(item);
        }
    }
}
