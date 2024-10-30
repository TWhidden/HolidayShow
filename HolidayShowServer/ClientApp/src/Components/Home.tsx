// src/components/Home.tsx

import React, { useCallback } from 'react';
import { observer } from 'mobx-react';
import { Typography, Box } from '@mui/material';
import { ApiStoreContextItem } from '../Stores/ApiStore';
import { AppStoreContextItem } from '../Stores/AppStore';
import ControlButton from './ControllerButton';

interface ButtonConfig {
    key: string | number;
    text: string; // Keeping it as string for consistency
    color: 'primary' | 'secondary' | 'success' | 'warning' | 'error' | 'info';
    onClick: () => void | Promise<void>; // Allowing for both synchronous and asynchronous handlers
  }

const Home: React.FC = observer(() => {
  const store = ApiStoreContextItem.useStore();
  const appStore = AppStoreContextItem.useStore();
  const { sets } = appStore;

  const restart = useCallback(() => {
    store.getApi().settingsRestartExecutionUpdate();
  }, [store]);

  const executeSet = useCallback(async (set: number) => {
    console.log('setting set to', set);
    await store.getApi().settingsCurrentSetUpdate(set);
    await store.getApi().settingsPlaybackOptionUpdate(2);
    await store.getApi().settingsRestartExecutionUpdate();
  }, [store]);

  const executeStart = useCallback(async () => {
    await store.getApi().settingsPlaybackOptionUpdate(2);
  }, [store]);

  const executeRandom = useCallback(async () => {
    await store.getApi().settingsPlaybackOptionUpdate(1);
  }, [store]);

  // Define static buttons
  const staticButtons: ButtonConfig[] = [
    { key: 'restart', text: 'RESTART SHOW', color: 'primary', onClick: restart },
    { key: 'stop', text: 'STOP EXECUTION', color: 'secondary', onClick: () => executeSet(0) },
    { key: 'start', text: 'START EXECUTION', color: 'success', onClick: executeStart },
    { key: 'random', text: 'START RANDOM', color: 'warning', onClick: executeRandom },
  ];

  // Define dynamic buttons based on sets
  const dynamicButtons: ButtonConfig[] = sets.map((set) => ({
    key: set.setId,
    text: set.setName ?? 'Unnamed Set', // Provide a default string if setName is null or undefined
    color: appStore.currentSet === set.setId ? 'secondary' : 'primary',
    onClick: () => executeSet(set.setId),
  }));

  // Combine all buttons
  const allButtons: ButtonConfig[] = [...staticButtons, ...dynamicButtons];

  return (
    <Box>
      {/* Header Section */}
      <Typography variant="h5" gutterBottom align="center">
        Welcome to the Holiday Show
      </Typography>
      <Typography variant="body1" gutterBottom align="center">
        Control the pre-programmed sequences quickly and efficiently.
      </Typography>

      {/* Buttons Section */}
      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: {
            xs: '1fr', // 1 column on extra-small screens
            sm: 'repeat(2, 1fr)', // 2 columns on small screens
            md: 'repeat(3, 1fr)', // 3 columns on medium and larger screens
          },
          gap: 2, // Space between buttons
          marginTop: 3,
        }}
      >
        {allButtons.map((button) => (
          <ControlButton
            key={button.key} // Unique key for each button
            text={button.text}
            color={button.color}
            onClick={button.onClick}
          />
        ))}
      </Box>
    </Box>
  );
});

export default Home;
