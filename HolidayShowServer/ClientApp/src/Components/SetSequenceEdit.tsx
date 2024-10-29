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

interface SetSequenceEditProps {
  sequence: SetSequences;
  onDelete: (sequence: SetSequences) => void;
}

const SetSequenceEdit: React.FC<SetSequenceEditProps> = observer(
  ({ sequence, onDelete }) => {
    const store = AppStoreContextItem.useStore();

    // State declarations
    const [devicePattern, setDevicePattern] = useState<{ label: string; value: number } | null>(null);
    const [effect, setEffect] = useState<{ label: string; value: number } | null>(null);
    const [onAt, setOnAt] = useState<string>(sequence.onAt.toString());

    // State to control when to save
    const [shouldSave, setShouldSave] = useState<boolean>(false);

    // Ref to track initial mount
    const isInitialMount = useRef(true);

    // Initialize the state based on the incoming sequence
    useEffect(() => {
      const selectedPattern = store.devicePatterns.find((p) => p.devicePatternId === sequence.devicePatternId) || null;
      const selectedEffect = store.deviceEffects.find((e) => e.effectId === sequence.effectId) || null;

      // Only set state if the incoming sequence differs from current state
      if (
        (selectedPattern ? selectedPattern.devicePatternId : null) !== (devicePattern ? devicePattern.value : null)
      ) {
        setDevicePattern(selectedPattern ? { label: selectedPattern.patternName ?? '', value: selectedPattern.devicePatternId } : null);
      }

      if (
        (selectedEffect ? selectedEffect.effectId : null) !== (effect ? effect.value : null)
      ) {
        setEffect(selectedEffect ? { label: selectedEffect.effectName ?? '', value: selectedEffect.effectId } : null);
      }

      const newOnAt = sequence.onAt.toString();
      if (newOnAt !== onAt) {
        setOnAt(newOnAt);
      }

      // Prevent initial mount from triggering a save
      if (isInitialMount.current) {
        isInitialMount.current = false;
      }
    }, [
      sequence.devicePatternId,
      sequence.effectId,
      sequence.onAt,
      store.devicePatterns,
      store.deviceEffects,
      devicePattern,
      effect,
      onAt
    ]);

    // Define handleSave inside useCallback to ensure stability
    const handleSave = useCallback(async () => {
      try {
        console.log('Saving Sequence...');
        console.log('Current State:');
        console.log('  onAt:', onAt);
        console.log('  devicePattern:', devicePattern);
        console.log('  effect:', effect);

        const updatedSequence: SetSequences = {
          ...sequence,
          onAt: Number(onAt),
          devicePatternId: devicePattern ? devicePattern.value : null,
          effectId: effect ? effect.value : null,
          setSequenceId: sequence.setSequenceId,
        };

        await store.updateSetSequence(updatedSequence.setSequenceId, updatedSequence); // Ensure this method exists
        store.clearError();
        console.log('Sequence saved successfully.');
      } catch (error: any) {
        store.setError(`Failed to save sequence: ${error.message}`);
        console.error('Error saving sequence:', error);
      }
    }, [sequence, onAt, devicePattern, effect, store]);

    // useEffect to handle save when shouldSave is true
    useEffect(() => {
      if (shouldSave) {
        handleSave();
        setShouldSave(false);
      }
    }, [shouldSave, handleSave]);

    return (
      <div style={{ display: 'flex', flexDirection: 'row', alignItems: 'center', marginBottom: '1rem' }}>
        {/* On At Field */}
        <TextField
          label="On At"
          value={onAt}
          type="number"
          onChange={(e: ChangeEvent<HTMLInputElement>) => {
            setOnAt(e.target.value);
            setShouldSave(true); // Trigger save after state update
          }}
          margin="normal"
          style={{ marginRight: '1rem', flex: 1, width: '30rem' }}
        />

        {/* Device Pattern ComboBox */}
        <ComboSelect
          isClearable={false}
          options={store.devicePatterns.map((p) => ({
            label: p.patternName ?? '',
            value: p.devicePatternId,
          }))}
          onChange={(selectedOption) => {
            console.log('Selected Device Pattern:', selectedOption);
            setDevicePattern(selectedOption);
            setShouldSave(true); // Trigger save after state update
          }}
          value={devicePattern}
          placeholder="Select Device Pattern"
          styles={{ container: (provided) => ({ ...provided, flex: 3, marginRight: '1rem' }) }}
        />

        {/* Effect ComboBox */}
        <ComboSelect
          isClearable={false}
          options={store.deviceEffects.map((e) => ({
            label: e.effectName ?? '',
            value: e.effectId,
          }))}
          onChange={(selectedOption) => {
            console.log('Selected Effect:', selectedOption);
            setEffect(selectedOption);
            setShouldSave(true); // Trigger save after state update
          }}
          value={effect}
          placeholder="Select Effect"
          styles={{ container: (provided) => ({ ...provided, flex: 3, marginRight: '1rem' }) }}
        />

        {/* Delete Button */}
        <Tooltip title="Delete Sequence">
          <IconButton onClick={() => onDelete(sequence)}>
            <DeleteIcon />
          </IconButton>
        </Tooltip>
      </div>
    );
  }
);

export default SetSequenceEdit;
