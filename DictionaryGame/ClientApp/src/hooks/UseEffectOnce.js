import { useEffect } from 'react';

/**
 * Calls useEffect with empty brackets for the second parameter to
 * ensure the given function is only executed once, when the component
 * loads.
 * 
 * @param {Function} fn
 */
export function useEffectOnce(fn) {
    useEffect(fn, []);
}
