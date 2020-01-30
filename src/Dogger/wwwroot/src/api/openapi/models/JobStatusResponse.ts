/* tslint:disable */
/* eslint-disable */
/**
 * General
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: v1
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */

import { exists, mapValues } from '../runtime';
/**
 * 
 * @export
 * @interface JobStatusResponse
 */
export interface JobStatusResponse {
    /**
     * 
     * @type {string}
     * @memberof JobStatusResponse
     */
    stateDescription?: string | null;
    /**
     * 
     * @type {boolean}
     * @memberof JobStatusResponse
     */
    isEnded?: boolean;
    /**
     * 
     * @type {boolean}
     * @memberof JobStatusResponse
     */
    isSucceeded?: boolean;
    /**
     * 
     * @type {boolean}
     * @memberof JobStatusResponse
     */
    isFailed?: boolean;
}

export function JobStatusResponseFromJSON(json: any): JobStatusResponse {
    return JobStatusResponseFromJSONTyped(json, false);
}

export function JobStatusResponseFromJSONTyped(json: any, ignoreDiscriminator: boolean): JobStatusResponse {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'stateDescription': !exists(json, 'stateDescription') ? undefined : json['stateDescription'],
        'isEnded': !exists(json, 'isEnded') ? undefined : json['isEnded'],
        'isSucceeded': !exists(json, 'isSucceeded') ? undefined : json['isSucceeded'],
        'isFailed': !exists(json, 'isFailed') ? undefined : json['isFailed'],
    };
}

export function JobStatusResponseToJSON(value?: JobStatusResponse | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'stateDescription': value.stateDescription,
        'isEnded': value.isEnded,
        'isSucceeded': value.isSucceeded,
        'isFailed': value.isFailed,
    };
}


