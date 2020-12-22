import { useEffect, useState } from 'react';
import { useAuth0 } from '@auth0/auth0-react';
import { fetcher } from './fetcher';
import { config } from '../../config';
import useSWR from 'swr';

export const useApi = (url, options = {}) => {
  const { audience, scope, ...fetchOptions } = options;
  const { getAccessTokenSilently } = useAuth0();
  const [state, setState] = useState({
    error: null,
    loading: true,
    data: null,
  });
  const [refreshIndex, setRefreshIndex] = useState(0);
  const [token, setToken] = useState('');

  const { data, error } = useSWR( token ? config.api.baseUrl + url : null, url => fetcher(url, token, fetchOptions), {
    refreshInterval: 0,
  })


  useEffect(() => {
    (async () => {
      try {
        const token = await getAccessTokenSilently({ audience, scope });
        setToken(token);
        setState({
          ...state,
          error,
          loading: false,
        });
      } catch (tokenError) {
        setState({
          ...state,
          error: tokenError,
          loading: false,
        });
        setToken('')
      }
    })();
  }, [refreshIndex]);

  useEffect(() => {
    setState({
      ...state,
      data,
      error,
      loading: error ? true : state.loading
    })
  }, [data, error])

  return {
    ...state,
    refresh: () => setRefreshIndex(refreshIndex + 1),
  };
};