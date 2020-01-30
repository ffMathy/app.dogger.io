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
 * @interface CommitPayload
 */
export interface CommitPayload {
    /**
     * 
     * @type {string}
     * @memberof CommitPayload
     */
    message?: string | null;
    /**
     * 
     * @type {Array<string>}
     * @memberof CommitPayload
     */
    added?: Array<string> | null;
    /**
     * 
     * @type {Array<string>}
     * @memberof CommitPayload
     */
    removed?: Array<string> | null;
    /**
     * 
     * @type {Array<string>}
     * @memberof CommitPayload
     */
    modified?: Array<string> | null;
}

export function CommitPayloadFromJSON(json: any): CommitPayload {
    return CommitPayloadFromJSONTyped(json, false);
}

export function CommitPayloadFromJSONTyped(json: any, ignoreDiscriminator: boolean): CommitPayload {
    if ((json === undefined) || (json === null)) {
        return json;
    }
    return {
        
        'message': !exists(json, 'message') ? undefined : json['message'],
        'added': !exists(json, 'added') ? undefined : json['added'],
        'removed': !exists(json, 'removed') ? undefined : json['removed'],
        'modified': !exists(json, 'modified') ? undefined : json['modified'],
    };
}

export function CommitPayloadToJSON(value?: CommitPayload | null): any {
    if (value === undefined) {
        return undefined;
    }
    if (value === null) {
        return null;
    }
    return {
        
        'message': value.message,
        'added': value.added,
        'removed': value.removed,
        'modified': value.modified,
    };
}


