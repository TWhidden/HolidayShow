// src/components/SetSequenceEdit.tsx

import React, { useEffect, useState, useRef, ChangeEvent, useCallback } from 'react';
import { observer } from 'mobx-react-lite';
import { AppStoreContextItem } from '../Stores/AppStore';
import { SetSequences } from '../Clients/Api';
import {
  TextField,
  IconButton,
  Tooltip,
} from '@mui/material';
import { Delete as DeleteIcon } from '@mui/icons-material';
import ComboSelect from 'react-select';
import debounce from 'lodash/debounce';

interface SetSequenceEditProps {
  sequence: SetSequences;
  onDelete: (sequence: SetSequences) => void;
}

const SetSequenceEdit: React.FC<SetSequenceEditProps> = observer(
  ({ sequence, onDelete }) => {
    const store = AppStoreContextItem.useStore();

    const [devicePattern, setDevicePattern] = useState<{ label: string; value: number } | null>(null);
    const [effect, setEffect] = useState<{ label: string; value: number } | null>(null);
    const [onAt, setOnAt] = useState<string>(sequence.onAt.toString());

    const isInitialMount = useRef(true);

    // Refs to hold the latest state values
    const onAtRef = useRef(onAt);
    const devicePatternRef = useRef(devicePattern);
    const effectRef = useRef(effect);

    // Update refs whenever state changes
    useEffect(() => {
      onAtRef.current = onAt;
    }, [onAt]);

    useEffect(() => {
      devicePatternRef.current = devicePattern;
    }, [devicePattern]);

    useEffect(() => {
      effectRef.current = effect;
    }, [effect]);

    // Initialize state when sequence prop changes
    useEffect(() => {
      const selectedPattern = store.devicePatterns.find(p => p.devicePatternId === sequence.devicePatternId) || null;
      const selectedEffect = store.deviceEffects.find(e => e.effectId === sequence.effectId) || null;

      setDevicePattern(selectedPattern ? { label: selectedPattern.patternName || '', value: selectedPattern.devicePatternId } : null);
      setEffect(selectedEffect ? { label: selectedEffect.effectName || '', value: selectedEffect.effectId } : null);
      setOnAt(sequence.onAt.toString());

      if (isInitialMount.current) {
        isInitialMount.current = false;
      }
    }, [sequence, store.devicePatterns, store.deviceEffects]);

    // Save function
    const handleSave = useCallback(async () => {
      try {
        console.log('Saving sequence...');
        console.log('onAt:', onAtRef.current);
        console.log('devicePattern:', devicePatternRef.current);
        console.log('effect:', effectRef.current);

        const updatedSequence: SetSequences = {
          ...sequence,
          onAt: Number(onAtRef.current),
          devicePatternId: devicePatternRef.current ? devicePatternRef.current.value : null,
          effectId: effectRef.current ? effectRef.current.value : null,
          setSequenceId: sequence.setSequenceId,
        };

        await store.updateSetSequence(updatedSequence.setSequenceId, updatedSequence);
        store.clearError();
      } catch (error: any) {
        store.setError(`Failed to save sequence: ${error.message}`);
      }
    }, [sequence, store]);

    // Ref to store the latest handleSave
    const handleSaveRef = useRef(handleSave);

    // Update the ref whenever handleSave changes
    useEffect(() => {
      handleSaveRef.current = handleSave;
    }, [handleSave]);

    // Debounced save function that calls the latest handleSave
    const debouncedSave = useRef(
      debounce(() => {
        handleSaveRef.current();
      }, 500)
    ).current;

    // Cleanup debounce on unmount
    useEffect(() => {
      return () => {
        console.log('cancel save');
        debouncedSave.cancel();
      };
    }, [debouncedSave]);

    // Handlers
    const handleOnAtChange = useCallback((e: ChangeEvent<HTMLInputElement>) => {
      console.log('handleOnAtChange:', e.target.value);
      setOnAt(e.target.value);
      if (!isInitialMount.current) {
        debouncedSave();
      }
    }, [debouncedSave]);

    const handleDevicePatternChange = useCallback((selectedOption: { label: string; value: number } | null) => {
      console.log('handleDevicePatternChange:', selectedOption);
      setDevicePattern(selectedOption);
      if (!isInitialMount.current) {
        debouncedSave();
      }
    }, [debouncedSave]);

    const handleEffectChange = useCallback((selectedOption: { label: string; value: number } | null) => {
      console.log('handleEffectChange:', selectedOption);
      setEffect(selectedOption);
      if (!isInitialMount.current) {
        debouncedSave();
      }
    }, [debouncedSave]);

    return (
      <div style={{ display: 'flex', alignItems: 'center', marginBottom: '0.25rem' }}>
        <TextField
          label="On At"
          value={onAt}
          type="number"
          onChange={handleOnAtChange}
          margin="dense"
          size="small"
          style={{ marginRight: '0.25rem', flex: 1, width: '30rem' }}
        />

        <ComboSelect
          isClearable={false}
          options={store.devicePatterns.map(p => ({ label: p.patternName || '', value: p.devicePatternId }))}
          onChange={handleDevicePatternChange}
          value={devicePattern}
          placeholder="Select Device Pattern"
          styles={{
            container: (provided) => ({ ...provided, flex: 3, marginRight: '0.25rem', minWidth: '150px' }),
            control: (provided) => ({ ...provided, minHeight: '30px' }),
            indicatorsContainer: (provided) => ({ ...provided, height: '30px' }),
            input: (provided) => ({ ...provided, margin: '0px' }),
          }}
        />

        <ComboSelect
          isClearable={false}
          options={store.deviceEffects.map(e => ({ label: e.effectName || '', value: e.effectId }))}
          onChange={handleEffectChange}
          value={effect}
          placeholder="Select Effect"
          styles={{
            container: (provided) => ({ ...provided, flex: 3, marginRight: '0.25rem', minWidth: '150px' }),
            control: (provided) => ({ ...provided, minHeight: '30px' }),
            indicatorsContainer: (provided) => ({ ...provided, height: '30px' }),
            input: (provided) => ({ ...provided, margin: '0px' }),
          }}
        />

        <Tooltip title="Delete Sequence">
          <IconButton
            onClick={() => onDelete(sequence)}
            size="small"
          >
            <DeleteIcon fontSize="small" />
          </IconButton>
        </Tooltip>
      </div>
    );
  }
);

export default SetSequenceEdit;
