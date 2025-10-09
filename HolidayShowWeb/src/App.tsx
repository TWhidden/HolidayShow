// App.tsx
import React, { useMemo, useState, useEffect } from 'react';
import { Routes, Route, BrowserRouter } from 'react-router-dom';

import Home from './Components/Home';
import DeviceManager from './Components/DeviceEditor';
import DevicePatternEditor from './Components/DevicePatternEditor';
import SetsEditor from './Components/SetsEditor';
import EffectsEditor from './Components/EffectsEditor';
import SettingsEditor from './Components/SettingsEditor';
import './App.css';
import { ApiStoreContextItem } from './Stores/ApiStore';
import { AppStoreContextItem } from './Stores/AppStore';
import Layout from './Components/Layout';
import { CssBaseline, PaletteMode, ThemeProvider, createTheme } from '@mui/material';

const App: React.FC = () => {
  // Manage the theme mode state
  const [mode, setMode] = useState<PaletteMode>('light');

  // Persist theme preference in localStorage
  useEffect(() => {
    const storedMode = localStorage.getItem('themeMode') as PaletteMode;
    if (storedMode) {
      setMode(storedMode);
    }
  }, []);

  // Toggle between light and dark mode
  const toggleTheme = () => {
    setMode((prevMode) => {
      const newMode = prevMode === 'light' ? 'dark' : 'light';
      localStorage.setItem('themeMode', newMode);
      return newMode;
    });
  };

  // Memoize the theme to optimize performance
  const theme = useMemo(
    () =>
      createTheme({
        palette: {
          mode,
          // You can customize the palette further here
          ...(mode === 'light'
            ? {
                // palette values for light mode
                primary: {
                  main: '#1976d2',
                },
                background: {
                  default: '#f5f5f5',
                  paper: '#ffffff',
                },
              }
            : {
                // palette values for dark mode
                primary: {
                  main: '#90caf9',
                },
                background: {
                  default: '#121212',
                  paper: '#1d1d1d',
                },
              }),
        },
      }),
    [mode]
  );

  return (
    <ThemeProvider theme={theme}>
      {/* CssBaseline applies a consistent baseline for styling */}
      <CssBaseline />
      <ApiStoreContextItem.ProviderComponent>
        <AppStoreContextItem.ProviderComponent>
          <BrowserRouter>
            {/* Pass toggleTheme and currentMode as props to Layout */}
            <Layout toggleTheme={toggleTheme} currentMode={mode}>
              <Routes>
                <Route path="/" element={<Home />} />
                <Route path="/DeviceEditor" element={<DeviceManager />} />
                <Route path="/DevicePatternEditor" element={<DevicePatternEditor />} />
                <Route path="/SetsEditor" element={<SetsEditor />} />
                <Route path="/EffectsEditor" element={<EffectsEditor />} />
                <Route path="/SettingsEditor" element={<SettingsEditor />} />
              </Routes>
            </Layout>
          </BrowserRouter>
        </AppStoreContextItem.ProviderComponent>
      </ApiStoreContextItem.ProviderComponent>
    </ThemeProvider>
  );
};

export default App;
