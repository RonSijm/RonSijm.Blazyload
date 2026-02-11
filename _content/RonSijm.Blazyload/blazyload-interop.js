// Blazyload JavaScript Interop for .NET 10+ fingerprint support
// This file provides functions to access the Blazor runtime configuration
// which contains the mapping between virtual assembly names and fingerprinted file names.

window.BlazyloadInterop = {
    // Cache the mapping to avoid repeated lookups
    _cachedMapping: null,
    // Enable debug logging
    _debugEnabled: false,

    /**
     * Enable or disable debug logging
     * @param {boolean} enabled - Whether to enable debug logging
     */
    setDebugEnabled: function (enabled) {
        this._debugEnabled = enabled;
    },

    /**
     * Log a debug message if debug is enabled
     * @param {string} message - The message to log
     */
    _debug: function (message, ...args) {
        if (this._debugEnabled) {
            console.log('[Blazyload] ' + message, ...args);
        }
    },

    /**
     * Gets the lazy assembly mapping from the Blazor runtime configuration.
     * Returns an object mapping virtualPath -> fingerprinted name.
     * @returns {Object} Dictionary of virtualPath to fingerprinted name
     */
    getLazyAssemblyMapping: function () {
        // Return cached mapping if available
        if (this._cachedMapping !== null) {
            this._debug('Returning cached mapping with', Object.keys(this._cachedMapping).length, 'entries');
            return this._cachedMapping;
        }

        try {
            // Try multiple ways to access the Blazor config
            // .NET 10 embeds the config in dotnet.js and exposes it through the runtime
            let config = null;
            let configSource = 'none';

            // Method 1: Try Blazor.runtime.getConfig() - Primary method for .NET 10+
            if (!config && typeof Blazor !== 'undefined' && Blazor.runtime && typeof Blazor.runtime.getConfig === 'function') {
                this._debug('Trying Blazor.runtime.getConfig()');
                config = Blazor.runtime.getConfig();
                if (config) {
                    configSource = 'Blazor.runtime.getConfig()';
                    this._debug('Got config from Blazor.runtime.getConfig()');
                }
            }

            // Method 2: Try Blazor._internal.getApplicationEnvironment
            if (!config && typeof Blazor !== 'undefined' && Blazor._internal) {
                this._debug('Trying Blazor._internal.getApplicationEnvironment()');
                const env = Blazor._internal.getApplicationEnvironment?.();
                if (env && env.config) {
                    config = env.config;
                    configSource = 'Blazor._internal.getApplicationEnvironment()';
                    this._debug('Got config from Blazor._internal');
                }
            }

            // Method 3: Try window.dotnetRuntime
            if (!config && typeof window.dotnetRuntime !== 'undefined') {
                this._debug('Trying window.dotnetRuntime.getConfig()');
                config = window.dotnetRuntime.getConfig?.();
                if (config) {
                    configSource = 'window.dotnetRuntime.getConfig()';
                    this._debug('Got config from window.dotnetRuntime');
                }
            }

            // Method 4: Try accessing through Module (Emscripten)
            if (!config && typeof Module !== 'undefined' && Module.config) {
                this._debug('Trying Module.config');
                config = Module.config;
                configSource = 'Module.config';
                this._debug('Got config from Module.config');
            }

            // Method 5: Try accessing through globalThis.__BLAZOR_WEBASSEMBLY__
            if (!config && typeof globalThis !== 'undefined' && globalThis.__BLAZOR_WEBASSEMBLY__) {
                this._debug('Trying globalThis.__BLAZOR_WEBASSEMBLY__');
                config = globalThis.__BLAZOR_WEBASSEMBLY__.config;
                if (config) {
                    configSource = 'globalThis.__BLAZOR_WEBASSEMBLY__';
                    this._debug('Got config from globalThis.__BLAZOR_WEBASSEMBLY__');
                }
            }

            if (!config) {
                this._debug('No config found from any source');
                this._cachedMapping = {};
                return this._cachedMapping;
            }

            this._debug('Config source:', configSource);
            this._debug('Config has resources:', !!config.resources);
            this._debug('Config has lazyAssembly:', !!(config.resources && config.resources.lazyAssembly));

            if (config && config.resources && config.resources.lazyAssembly) {
                const mapping = {};
                const lazyAssemblies = config.resources.lazyAssembly;

                this._debug('lazyAssembly type:', Array.isArray(lazyAssemblies) ? 'array' : typeof lazyAssemblies);

                // Handle both array format (new) and dictionary format (old)
                if (Array.isArray(lazyAssemblies)) {
                    // .NET 10 format: array of { virtualPath, name, integrity, cache }
                    this._debug('Processing array format with', lazyAssemblies.length, 'entries');
                    for (const asset of lazyAssemblies) {
                        if (asset.virtualPath && asset.name) {
                            mapping[asset.virtualPath] = asset.name;
                            this._debug('Mapped:', asset.virtualPath, '->', asset.name);
                        }
                    }
                } else if (typeof lazyAssemblies === 'object') {
                    // Older format: dictionary of name -> hash
                    // In this case, the key IS the filename, no fingerprinting
                    this._debug('Processing dictionary format with', Object.keys(lazyAssemblies).length, 'entries');
                    for (const key of Object.keys(lazyAssemblies)) {
                        mapping[key] = key;
                    }
                }

                this._debug('Created mapping with', Object.keys(mapping).length, 'entries');
                this._cachedMapping = mapping;
                return mapping;
            }
        } catch (e) {
            console.warn('Blazyload: Error getting lazy assembly mapping', e);
        }

        // Return empty mapping if we couldn't find the config
        this._cachedMapping = {};
        return this._cachedMapping;
    },

    /**
     * Gets the fingerprinted name for a specific assembly.
     * @param {string} virtualPath - The virtual/logical assembly name (e.g., "MyAssembly.wasm")
     * @returns {string} The fingerprinted name, or the original name if not found
     */
    getFingerprintedName: function (virtualPath) {
        const mapping = this.getLazyAssemblyMapping();
        return mapping[virtualPath] || virtualPath;
    },

    /**
     * Clears the cached mapping. Useful if the config changes at runtime.
     */
    clearCache: function () {
        this._cachedMapping = null;
    },

    /**
     * Checks if fingerprinting is being used (i.e., if any mappings differ from their virtual paths).
     * @returns {boolean} True if fingerprinting is detected
     */
    isFingerprintingEnabled: function () {
        const mapping = this.getLazyAssemblyMapping();
        for (const virtualPath of Object.keys(mapping)) {
            if (mapping[virtualPath] !== virtualPath) {
                return true;
            }
        }
        return false;
    },

    /**
     * Diagnostic function to check what Blazor APIs are available.
     * Useful for debugging config access issues.
     * @returns {Object} Diagnostic information about available APIs
     */
    getDiagnostics: function () {
        const diag = {
            blazorDefined: typeof Blazor !== 'undefined',
            blazorRuntime: false,
            blazorRuntimeGetConfig: false,
            blazorInternal: false,
            blazorInternalGetAppEnv: false,
            windowDotnetRuntime: typeof window.dotnetRuntime !== 'undefined',
            moduleDefined: typeof Module !== 'undefined',
            moduleConfig: false,
            globalThisBlazorWasm: typeof globalThis !== 'undefined' && !!globalThis.__BLAZOR_WEBASSEMBLY__,
            configFound: false,
            configHasResources: false,
            configHasLazyAssembly: false,
            lazyAssemblyCount: 0,
            lazyAssemblyType: 'none'
        };

        if (typeof Blazor !== 'undefined') {
            diag.blazorRuntime = !!Blazor.runtime;
            if (Blazor.runtime) {
                diag.blazorRuntimeGetConfig = typeof Blazor.runtime.getConfig === 'function';
            }
            diag.blazorInternal = !!Blazor._internal;
            if (Blazor._internal) {
                diag.blazorInternalGetAppEnv = typeof Blazor._internal.getApplicationEnvironment === 'function';
            }
        }

        if (typeof Module !== 'undefined') {
            diag.moduleConfig = !!Module.config;
        }

        // Try to get config
        let config = null;
        if (diag.blazorRuntimeGetConfig) {
            try {
                config = Blazor.runtime.getConfig();
            } catch (e) {
                diag.configError = e.message;
            }
        }

        if (config) {
            diag.configFound = true;
            diag.configHasResources = !!config.resources;
            if (config.resources) {
                diag.configHasLazyAssembly = !!config.resources.lazyAssembly;
                if (config.resources.lazyAssembly) {
                    const la = config.resources.lazyAssembly;
                    diag.lazyAssemblyType = Array.isArray(la) ? 'array' : typeof la;
                    diag.lazyAssemblyCount = Array.isArray(la) ? la.length : Object.keys(la).length;
                }
            }
        }

        return diag;
    }
};
