/* eslint-disable */
/* tslint:disable */
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

export interface AudioOptions {
  /** @format int32 */
  audioId: number;
  name?: string | null;
  fileName?: string | null;
  /** @format int32 */
  audioDuration?: number;
  isNotVisable?: boolean;
  displayName?: string | null;
}

export interface DeviceEffects {
  /** @format int32 */
  effectId: number;
  effectName?: string | null;
  instructionMetaData?: string | null;
  /** @format int32 */
  duration?: number;
  /** @format int32 */
  effectInstructionId: number;
  timeOn?: string | null;
  timeOff?: string | null;
}

export interface DeviceIoPorts {
  /** @format int32 */
  deviceIoPortId: number;
  /** @format int32 */
  deviceId?: number;
  /** @format int32 */
  commandPin?: number;
  description?: string | null;
  isNotVisable?: boolean;
  isDanger: boolean;
  devices?: Devices;
}

export interface DevicePatternSequences {
  /** @format int32 */
  devicePatternSeqenceId: number;
  /** @format int32 */
  devicePatternId?: number;
  /** @format int32 */
  onAt: number;
  /** @format int32 */
  duration?: number;
  /** @format int32 */
  audioId?: number;
  /** @format int32 */
  deviceIoPortId?: number;
}

export interface DevicePatterns {
  /** @format int32 */
  devicePatternId: number;
  /** @format int32 */
  deviceId?: number;
  patternName?: string | null;
}

export interface Devices {
  /** @format int32 */
  deviceId: number;
  name?: string | null;
}

export interface EffectInstructionsAvailable {
  /** @format int32 */
  effectInstructionId: number;
  displayName?: string | null;
  instructionName?: string | null;
  instructionsForUse?: string | null;
  isDisabled?: boolean;
}

export interface SetSequences {
  /** @format int32 */
  setSequenceId: number;
  /** @format int32 */
  setId?: number;
  /** @format int32 */
  onAt: number;
  /** @format int32 */
  devicePatternId?: number | null;
  /** @format int32 */
  effectId?: number | null;
}

export interface Sets {
  /** @format int32 */
  setId: number;
  setName?: string | null;
  isDisabled?: boolean;
}

export interface Settings {
  /** @minLength 1 */
  settingName: string;
  valueString?: string | null;
  /** @format double */
  valueDouble?: number;
}

export type QueryParamsType = Record<string | number, any>;
export type ResponseFormat = keyof Omit<Body, "body" | "bodyUsed">;

export interface FullRequestParams extends Omit<RequestInit, "body"> {
  /** set parameter to `true` for call `securityWorker` for this request */
  secure?: boolean;
  /** request path */
  path: string;
  /** content type of request body */
  type?: ContentType;
  /** query params */
  query?: QueryParamsType;
  /** format of response (i.e. response.json() -> format: "json") */
  format?: ResponseFormat;
  /** request body */
  body?: unknown;
  /** base url */
  baseUrl?: string;
  /** request cancellation token */
  cancelToken?: CancelToken;
}

export type RequestParams = Omit<FullRequestParams, "body" | "method" | "query" | "path">;

export interface ApiConfig<SecurityDataType = unknown> {
  baseUrl?: string;
  baseApiParams?: Omit<RequestParams, "baseUrl" | "cancelToken" | "signal">;
  securityWorker?: (securityData: SecurityDataType | null) => Promise<RequestParams | void> | RequestParams | void;
  customFetch?: typeof fetch;
}

export interface HttpResponse<D extends unknown, E extends unknown = unknown> extends Response {
  data: D;
  error: E;
}

type CancelToken = Symbol | string | number;

export enum ContentType {
  Json = "application/json",
  FormData = "multipart/form-data",
  UrlEncoded = "application/x-www-form-urlencoded",
  Text = "text/plain",
}

export class HttpClient<SecurityDataType = unknown> {
  public baseUrl: string = "";
  private securityData: SecurityDataType | null = null;
  private securityWorker?: ApiConfig<SecurityDataType>["securityWorker"];
  private abortControllers = new Map<CancelToken, AbortController>();
  private customFetch = (...fetchParams: Parameters<typeof fetch>) => fetch(...fetchParams);

  private baseApiParams: RequestParams = {
    credentials: "same-origin",
    headers: {},
    redirect: "follow",
    referrerPolicy: "no-referrer",
  };

  constructor(apiConfig: ApiConfig<SecurityDataType> = {}) {
    Object.assign(this, apiConfig);
  }

  public setSecurityData = (data: SecurityDataType | null) => {
    this.securityData = data;
  };

  protected encodeQueryParam(key: string, value: any) {
    const encodedKey = encodeURIComponent(key);
    return `${encodedKey}=${encodeURIComponent(typeof value === "number" ? value : `${value}`)}`;
  }

  protected addQueryParam(query: QueryParamsType, key: string) {
    return this.encodeQueryParam(key, query[key]);
  }

  protected addArrayQueryParam(query: QueryParamsType, key: string) {
    const value = query[key];
    return value.map((v: any) => this.encodeQueryParam(key, v)).join("&");
  }

  protected toQueryString(rawQuery?: QueryParamsType): string {
    const query = rawQuery || {};
    const keys = Object.keys(query).filter((key) => "undefined" !== typeof query[key]);
    return keys
      .map((key) => (Array.isArray(query[key]) ? this.addArrayQueryParam(query, key) : this.addQueryParam(query, key)))
      .join("&");
  }

  protected addQueryParams(rawQuery?: QueryParamsType): string {
    const queryString = this.toQueryString(rawQuery);
    return queryString ? `?${queryString}` : "";
  }

  private contentFormatters: Record<ContentType, (input: any) => any> = {
    [ContentType.Json]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string") ? JSON.stringify(input) : input,
    [ContentType.Text]: (input: any) => (input !== null && typeof input !== "string" ? JSON.stringify(input) : input),
    [ContentType.FormData]: (input: any) =>
      Object.keys(input || {}).reduce((formData, key) => {
        const property = input[key];
        formData.append(
          key,
          property instanceof Blob
            ? property
            : typeof property === "object" && property !== null
              ? JSON.stringify(property)
              : `${property}`,
        );
        return formData;
      }, new FormData()),
    [ContentType.UrlEncoded]: (input: any) => this.toQueryString(input),
  };

  protected mergeRequestParams(params1: RequestParams, params2?: RequestParams): RequestParams {
    return {
      ...this.baseApiParams,
      ...params1,
      ...(params2 || {}),
      headers: {
        ...(this.baseApiParams.headers || {}),
        ...(params1.headers || {}),
        ...((params2 && params2.headers) || {}),
      },
    };
  }

  protected createAbortSignal = (cancelToken: CancelToken): AbortSignal | undefined => {
    if (this.abortControllers.has(cancelToken)) {
      const abortController = this.abortControllers.get(cancelToken);
      if (abortController) {
        return abortController.signal;
      }
      return void 0;
    }

    const abortController = new AbortController();
    this.abortControllers.set(cancelToken, abortController);
    return abortController.signal;
  };

  public abortRequest = (cancelToken: CancelToken) => {
    const abortController = this.abortControllers.get(cancelToken);

    if (abortController) {
      abortController.abort();
      this.abortControllers.delete(cancelToken);
    }
  };

  public request = async <T = any, E = any>({
    body,
    secure,
    path,
    type,
    query,
    format,
    baseUrl,
    cancelToken,
    ...params
  }: FullRequestParams): Promise<T> => {
    const secureParams =
      ((typeof secure === "boolean" ? secure : this.baseApiParams.secure) &&
        this.securityWorker &&
        (await this.securityWorker(this.securityData))) ||
      {};
    const requestParams = this.mergeRequestParams(params, secureParams);
    const queryString = query && this.toQueryString(query);
    const payloadFormatter = this.contentFormatters[type || ContentType.Json];
    const responseFormat = format || requestParams.format;

    return this.customFetch(`${baseUrl || this.baseUrl || ""}${path}${queryString ? `?${queryString}` : ""}`, {
      ...requestParams,
      headers: {
        ...(requestParams.headers || {}),
        ...(type && type !== ContentType.FormData ? { "Content-Type": type } : {}),
      },
      signal: (cancelToken ? this.createAbortSignal(cancelToken) : requestParams.signal) || null,
      body: typeof body === "undefined" || body === null ? null : payloadFormatter(body),
    }).then(async (response) => {
      const r = response.clone() as HttpResponse<T, E>;
      r.data = null as unknown as T;
      r.error = null as unknown as E;

      const data = !responseFormat
        ? r
        : await response[responseFormat]()
            .then((data) => {
              if (r.ok) {
                r.data = data;
              } else {
                r.error = data;
              }
              return r;
            })
            .catch((e) => {
              r.error = e;
              return r;
            });

      if (cancelToken) {
        this.abortControllers.delete(cancelToken);
      }

      if (!response.ok) throw data;
      return data.data;
    });
  };
}

/**
 * @title HolidayShowServer
 * @version 1.0
 */
export class Api<SecurityDataType extends unknown> extends HttpClient<SecurityDataType> {
  api = {
    /**
     * No description
     *
     * @tags AudioOptions
     * @name AudioOptionsList
     * @request GET:/api/AudioOptions
     */
    audioOptionsList: (params: RequestParams = {}) =>
      this.request<AudioOptions[], any>({
        path: `/api/AudioOptions`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags AudioOptions
     * @name AudioOptionsCreate
     * @request POST:/api/AudioOptions
     */
    audioOptionsCreate: (data: AudioOptions, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/AudioOptions`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags AudioOptions
     * @name AudioOptionsDetail
     * @request GET:/api/AudioOptions/{id}
     */
    audioOptionsDetail: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/AudioOptions/${id}`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags AudioOptions
     * @name AudioOptionsUpdate
     * @request PUT:/api/AudioOptions/{id}
     */
    audioOptionsUpdate: (id: number, data: AudioOptions, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/AudioOptions/${id}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags AudioOptions
     * @name AudioOptionsDelete
     * @request DELETE:/api/AudioOptions/{id}
     */
    audioOptionsDelete: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/AudioOptions/${id}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DeviceEffects
     * @name DeviceEffectsList
     * @request GET:/api/DeviceEffects
     */
    deviceEffectsList: (params: RequestParams = {}) =>
      this.request<DeviceEffects[], any>({
        path: `/api/DeviceEffects`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DeviceEffects
     * @name DeviceEffectsCreate
     * @request POST:/api/DeviceEffects
     */
    deviceEffectsCreate: (data: DeviceEffects, params: RequestParams = {}) =>
      this.request<DeviceEffects, any>({
        path: `/api/DeviceEffects`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DeviceEffects
     * @name DeviceEffectsDetail
     * @request GET:/api/DeviceEffects/{id}
     */
    deviceEffectsDetail: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/DeviceEffects/${id}`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DeviceEffects
     * @name DeviceEffectsUpdate
     * @request PUT:/api/DeviceEffects/{id}
     */
    deviceEffectsUpdate: (id: number, data: DeviceEffects, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/DeviceEffects/${id}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags DeviceEffects
     * @name DeviceEffectsDelete
     * @request DELETE:/api/DeviceEffects/{id}
     */
    deviceEffectsDelete: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/DeviceEffects/${id}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DeviceIoPorts
     * @name DeviceIoPortsList
     * @request GET:/api/DeviceIoPorts
     */
    deviceIoPortsList: (params: RequestParams = {}) =>
      this.request<DeviceIoPorts[], any>({
        path: `/api/DeviceIoPorts`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DeviceIoPorts
     * @name DeviceIoPortsCreate
     * @request POST:/api/DeviceIoPorts
     */
    deviceIoPortsCreate: (data: DeviceIoPorts, params: RequestParams = {}) =>
      this.request<DeviceIoPorts, any>({
        path: `/api/DeviceIoPorts`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DeviceIoPorts
     * @name DeviceIoPortsDetail
     * @request GET:/api/DeviceIoPorts/{id}
     */
    deviceIoPortsDetail: (id: number, params: RequestParams = {}) =>
      this.request<DeviceIoPorts, any>({
        path: `/api/DeviceIoPorts/${id}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DeviceIoPorts
     * @name DeviceIoPortsUpdate
     * @request PUT:/api/DeviceIoPorts/{id}
     */
    deviceIoPortsUpdate: (id: number, data: DeviceIoPorts, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/DeviceIoPorts/${id}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags DeviceIoPorts
     * @name DeviceIoPortsDelete
     * @request DELETE:/api/DeviceIoPorts/{id}
     */
    deviceIoPortsDelete: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/DeviceIoPorts/${id}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DeviceIoPorts
     * @name DeviceIoPortsByDeviceIdDetail
     * @request GET:/api/DeviceIoPorts/ByDeviceId/{deviceId}
     */
    deviceIoPortsByDeviceIdDetail: (deviceId: number, params: RequestParams = {}) =>
      this.request<DeviceIoPorts[], any>({
        path: `/api/DeviceIoPorts/ByDeviceId/${deviceId}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DeviceIoPorts
     * @name DeviceIoPortsPutDeviceIoPortIdentifyUpdate
     * @request PUT:/api/DeviceIoPorts/PutDeviceIoPortIdentify/{id}
     */
    deviceIoPortsPutDeviceIoPortIdentifyUpdate: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/DeviceIoPorts/PutDeviceIoPortIdentify/${id}`,
        method: "PUT",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DevicePatterns
     * @name DevicePatternsList
     * @request GET:/api/DevicePatterns
     */
    devicePatternsList: (params: RequestParams = {}) =>
      this.request<DevicePatterns[], any>({
        path: `/api/DevicePatterns`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DevicePatterns
     * @name DevicePatternsCreate
     * @request POST:/api/DevicePatterns
     */
    devicePatternsCreate: (data: DevicePatterns, params: RequestParams = {}) =>
      this.request<DevicePatterns, any>({
        path: `/api/DevicePatterns`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DevicePatterns
     * @name DevicePatternsDetail
     * @request GET:/api/DevicePatterns/{id}
     */
    devicePatternsDetail: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/DevicePatterns/${id}`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DevicePatterns
     * @name DevicePatternsUpdate
     * @request PUT:/api/DevicePatterns/{id}
     */
    devicePatternsUpdate: (id: number, data: DevicePatterns, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/DevicePatterns/${id}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags DevicePatterns
     * @name DevicePatternsDelete
     * @request DELETE:/api/DevicePatterns/{id}
     */
    devicePatternsDelete: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/DevicePatterns/${id}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DevicePatterns
     * @name DevicePatternsGetDevicePatternsByDeviceIdDetail
     * @request GET:/api/DevicePatterns/GetDevicePatternsByDeviceId/{id}
     */
    devicePatternsGetDevicePatternsByDeviceIdDetail: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/DevicePatterns/GetDevicePatternsByDeviceId/${id}`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DevicePatternSequences
     * @name DevicePatternSequencesList
     * @request GET:/api/DevicePatternSequences
     */
    devicePatternSequencesList: (params: RequestParams = {}) =>
      this.request<DevicePatternSequences[], any>({
        path: `/api/DevicePatternSequences`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DevicePatternSequences
     * @name DevicePatternSequencesSequenceByPatternIdDetail
     * @request GET:/api/DevicePatternSequences/SequenceByPatternId/{id}
     */
    devicePatternSequencesSequenceByPatternIdDetail: (id: number, params: RequestParams = {}) =>
      this.request<DevicePatternSequences[], any>({
        path: `/api/DevicePatternSequences/SequenceByPatternId/${id}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DevicePatternSequences
     * @name DevicePatternSequencesDetail
     * @request GET:/api/DevicePatternSequences/{id}
     */
    devicePatternSequencesDetail: (id: number, params: RequestParams = {}) =>
      this.request<DevicePatternSequences, any>({
        path: `/api/DevicePatternSequences/${id}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DevicePatternSequences
     * @name DevicePatternSequencesUpdate
     * @request PUT:/api/DevicePatternSequences/{id}
     */
    devicePatternSequencesUpdate: (id: number, data: DevicePatternSequences, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/DevicePatternSequences/${id}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags DevicePatternSequences
     * @name DevicePatternSequencesDelete
     * @request DELETE:/api/DevicePatternSequences/{id}
     */
    devicePatternSequencesDelete: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/DevicePatternSequences/${id}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags DevicePatternSequences
     * @name DevicePatternSequencesCreate
     * @request POST:/api/DevicePatternSequences/{deviceId}
     */
    devicePatternSequencesCreate: (deviceId: number, data: DevicePatternSequences, params: RequestParams = {}) =>
      this.request<DevicePatternSequences, any>({
        path: `/api/DevicePatternSequences/${deviceId}`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Devices
     * @name DevicesList
     * @request GET:/api/Devices
     */
    devicesList: (params: RequestParams = {}) =>
      this.request<Devices[], any>({
        path: `/api/Devices`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Devices
     * @name DevicesCreate
     * @request POST:/api/Devices
     */
    devicesCreate: (data: Devices, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/Devices`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Devices
     * @name DevicesDetail
     * @request GET:/api/Devices/{id}
     */
    devicesDetail: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/Devices/${id}`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Devices
     * @name DevicesUpdate
     * @request PUT:/api/Devices/{id}
     */
    devicesUpdate: (id: number, data: Devices, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/Devices/${id}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Devices
     * @name DevicesDelete
     * @request DELETE:/api/Devices/{id}
     */
    devicesDelete: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/Devices/${id}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags EffectInstructionsAvailables
     * @name EffectInstructionsAvailablesList
     * @request GET:/api/EffectInstructionsAvailables
     */
    effectInstructionsAvailablesList: (params: RequestParams = {}) =>
      this.request<EffectInstructionsAvailable[], any>({
        path: `/api/EffectInstructionsAvailables`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags EffectInstructionsAvailables
     * @name EffectInstructionsAvailablesCreate
     * @request POST:/api/EffectInstructionsAvailables
     */
    effectInstructionsAvailablesCreate: (data: EffectInstructionsAvailable, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/EffectInstructionsAvailables`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags EffectInstructionsAvailables
     * @name EffectInstructionsAvailablesDetail
     * @request GET:/api/EffectInstructionsAvailables/{id}
     */
    effectInstructionsAvailablesDetail: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/EffectInstructionsAvailables/${id}`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags EffectInstructionsAvailables
     * @name EffectInstructionsAvailablesUpdate
     * @request PUT:/api/EffectInstructionsAvailables/{id}
     */
    effectInstructionsAvailablesUpdate: (id: number, data: EffectInstructionsAvailable, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/EffectInstructionsAvailables/${id}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags EffectInstructionsAvailables
     * @name EffectInstructionsAvailablesDelete
     * @request DELETE:/api/EffectInstructionsAvailables/{id}
     */
    effectInstructionsAvailablesDelete: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/EffectInstructionsAvailables/${id}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Sets
     * @name SetsList
     * @request GET:/api/Sets
     */
    setsList: (params: RequestParams = {}) =>
      this.request<Sets[], any>({
        path: `/api/Sets`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Sets
     * @name SetsCreate
     * @request POST:/api/Sets
     */
    setsCreate: (data: Sets, params: RequestParams = {}) =>
      this.request<Sets, any>({
        path: `/api/Sets`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Sets
     * @name SetsDetail
     * @request GET:/api/Sets/{id}
     */
    setsDetail: (id: number, params: RequestParams = {}) =>
      this.request<Sets[], any>({
        path: `/api/Sets/${id}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Sets
     * @name SetsUpdate
     * @request PUT:/api/Sets/{id}
     */
    setsUpdate: (id: number, data: Sets, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/Sets/${id}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Sets
     * @name SetsDelete
     * @request DELETE:/api/Sets/{id}
     */
    setsDelete: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/Sets/${id}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags SetSequences
     * @name SetSequencesList
     * @request GET:/api/SetSequences
     */
    setSequencesList: (params: RequestParams = {}) =>
      this.request<SetSequences[], any>({
        path: `/api/SetSequences`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags SetSequences
     * @name SetSequencesCreate
     * @request POST:/api/SetSequences
     */
    setSequencesCreate: (data: SetSequences, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/SetSequences`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags SetSequences
     * @name SetSequencesSetSequencesBySetIdDetail
     * @request GET:/api/SetSequences/SetSequencesBySetId/{setId}
     */
    setSequencesSetSequencesBySetIdDetail: (setId: number, params: RequestParams = {}) =>
      this.request<SetSequences[], any>({
        path: `/api/SetSequences/SetSequencesBySetId/${setId}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags SetSequences
     * @name SetSequencesDetail
     * @request GET:/api/SetSequences/{id}
     */
    setSequencesDetail: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/SetSequences/${id}`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags SetSequences
     * @name SetSequencesUpdate
     * @request PUT:/api/SetSequences/{id}
     */
    setSequencesUpdate: (id: number, data: SetSequences, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/SetSequences/${id}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags SetSequences
     * @name SetSequencesDelete
     * @request DELETE:/api/SetSequences/{id}
     */
    setSequencesDelete: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/SetSequences/${id}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Settings
     * @name SettingsList
     * @request GET:/api/Settings
     */
    settingsList: (params: RequestParams = {}) =>
      this.request<Settings[], any>({
        path: `/api/Settings`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Settings
     * @name SettingsCreate
     * @request POST:/api/Settings
     */
    settingsCreate: (data: Settings, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/Settings`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Settings
     * @name SettingsDetail
     * @request GET:/api/Settings/{id}
     */
    settingsDetail: (id: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/Settings/${id}`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Settings
     * @name SettingsUpdate
     * @request PUT:/api/Settings/{id}
     */
    settingsUpdate: (id: string, data: Settings, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/Settings/${id}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Settings
     * @name SettingsDelete
     * @request DELETE:/api/Settings/{id}
     */
    settingsDelete: (id: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/Settings/${id}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Settings
     * @name SettingsRestartExecutionUpdate
     * @request PUT:/api/Settings/RestartExecution
     */
    settingsRestartExecutionUpdate: (params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/Settings/RestartExecution`,
        method: "PUT",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Settings
     * @name SettingsPlaybackOptionUpdate
     * @request PUT:/api/Settings/PlaybackOption/{playbackOption}
     */
    settingsPlaybackOptionUpdate: (playbackOption: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/Settings/PlaybackOption/${playbackOption}`,
        method: "PUT",
        ...params,
      }),
  };
}
