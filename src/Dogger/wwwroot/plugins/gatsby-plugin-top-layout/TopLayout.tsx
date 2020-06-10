import React, { useState, useMemo, PropsWithChildren } from 'react';
import { ThemeProvider } from "@material-ui/styles";
import {
  CssBaseline, useMediaQuery, createMuiTheme, PaletteType
} from "@material-ui/core";

import './TopLayout.css';

import {NavigationBar} from '../../src/components/NavigationBar';
import { Auth0Provider } from '../../src/auth/Auth0Provider';

import { auth0Config, onRedirectCallback } from '../../src/setup/auth';
import { localStorageGet, localStorageSet } from '../../src/helpers/gatsby-compatibility';

import {Helmet} from "react-helmet";

export default ({children}: PropsWithChildren<any>) => {
  const themeFromStorage = localStorageGet("theme");

  const prefersDarkModeFromMediaQuery = useMediaQuery('(prefers-color-scheme: dark)');
  const prefersLightModeFromMediaQuery = useMediaQuery('(prefers-color-scheme: light)');
  const [themeName, setThemeName] = useState(themeFromStorage);

  const isPreferredThemeReady =
    prefersDarkModeFromMediaQuery ||
    prefersLightModeFromMediaQuery;

  const theme = useMemo(
    () =>
      createMuiTheme({
        palette: {
          type: 
            (themeName || 
            (isPreferredThemeReady && prefersDarkModeFromMediaQuery ? 'dark' : 'light')) as PaletteType,
          primary: {
            light: '#b88e68',
            main: '#86613d',
            dark: '#573715',
            contrastText: '#ffffff',
          },
          secondary: {
            light: '#ffffc7',
            main: '#e0cd96',
            dark: '#ad9c68',
            contrastText: '#000000',
          }
        },
      }),
    [
      themeName, 
      prefersDarkModeFromMediaQuery, 
      isPreferredThemeReady
    ],
  );

  if(!isPreferredThemeReady)
    return null;
  
  return (
    <>
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Auth0Provider
        domain={auth0Config.domain}
        client_id={auth0Config.clientId}
        redirect_uri={typeof window !== "undefined" && window.location.origin}
        audience={auth0Config.audience}
        onRedirectCallback={onRedirectCallback}
      >
        <>
        <Helmet>
            <title>Docker technologies - Dogger</title>
            <meta name="robots" content="all" />
            <meta 
                name="description" 
                content="Dogger has services and products to aid you in your Docker-related workflow." />
        </Helmet>
        <NavigationBar onThemeToggle={() => {
            const newThemeName = theme.palette.type === "dark" ? "light" : "dark";
            localStorageSet("theme", newThemeName);
            setThemeName(newThemeName);
        }} />
        <div style={{ display: "flex", flexDirection: 'column', flexGrow: 1 }}>
            {children}
        </div>
        </>
      </Auth0Provider>
    </ThemeProvider>
    </>
  );
}