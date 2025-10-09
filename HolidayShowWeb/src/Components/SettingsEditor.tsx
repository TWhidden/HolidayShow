// SettingsEditor.tsx
import React, { useEffect } from 'react';
import { observer, useLocalObservable } from 'mobx-react-lite';
import { runInAction } from 'mobx';
import { AppStoreContextItem } from '../Stores/AppStore';
import { Settings } from '../Clients/Api';
import {
    TextField,
    FormControlLabel,
    Switch,
    Box,
    Stack,
    Typography,
    Divider,
} from '@mui/material';
import { TimePicker, LocalizationProvider } from '@mui/x-date-pickers';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs, { Dayjs } from 'dayjs';

// Constants for setting names
const delayMs = "SetDelayMs";
const audioTimeOffAt = "AudioTimeOffAt";
const audioTimeOnAt = "AudioTimeOnAt";
const timeOffAt = "TimeOffAt";
const timeOnAt = "TimeOnAt";
const fileBasePath = "FileBasePath";
const isAudioEnabled = "IsAudioEnabled";
const isDangerEnabled = "IsDangerEnabled";

const SettingsEditor: React.FC = observer(() => {
    const store = AppStoreContextItem.useStore();

    const state = useLocalObservable(() => ({
        delayBetweenSets: 0 as number,
        onAt: "" as string,
        offAt: "" as string,
        audioOnAt: "" as string,
        audioOffAt: "" as string,
        enableDangerPins: 0 as number,
        enableAudio: 0 as number,
        audioFileLocation: "" as string,

        // Actions to update state
        setDelayBetweenSets(value: number) {
            this.delayBetweenSets = value;
        },
        setOnAt(value: string) {
            this.onAt = value;
        },
        setOffAt(value: string) {
            this.offAt = value;
        },
        setAudioOnAt(value: string) {
            this.audioOnAt = value;
        },
        setAudioOffAt(value: string) {
            this.audioOffAt = value;
        },
        setEnableDangerPins(value: number) {
            this.enableDangerPins = value;
        },
        setEnableAudio(value: number) {
            this.enableAudio = value;
        },
        setAudioFileLocation(value: string) {
            this.audioFileLocation = value;
        },
    }));

    useEffect(() => {
        getAllSetting();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const getAllSetting = async () => {
        try {
            // Assuming settings are already loaded in store.settings
            const delayBetweenSets = store.settings.find(x => x.settingName === delayMs)?.valueDouble ?? 0;
            const onAt = store.settings.find(x => x.settingName === timeOnAt)?.valueString ?? "";
            const offAt = store.settings.find(x => x.settingName === timeOffAt)?.valueString ?? "";
            const audioOnAtValue = store.settings.find(x => x.settingName === audioTimeOnAt)?.valueString ?? "";
            const audioOffAtValue = store.settings.find(x => x.settingName === audioTimeOffAt)?.valueString ?? "";
            const enableDangerPins = store.settings.find(x => x.settingName === isDangerEnabled)?.valueDouble ?? 0;
            const enableAudio = store.settings.find(x => x.settingName === isAudioEnabled)?.valueDouble ?? 0;
            const audioFileLocation = store.settings.find(x => x.settingName === fileBasePath)?.valueString ?? "";

            runInAction(() => {
                state.delayBetweenSets = delayBetweenSets;
                state.onAt = onAt;
                state.offAt = offAt;
                state.audioOnAt = audioOnAtValue;
                state.audioOffAt = audioOffAtValue;
                state.enableDangerPins = enableDangerPins;
                state.enableAudio = enableAudio;
                state.audioFileLocation = audioFileLocation;
            });

        } catch (e: any) {
            store.setError(e.message);
        }
    };

    const handleSaveSetting = async (settingName: string, valueString: string, valueDouble: number) => {
        try {
            const existingSetting = store.settings.find(x => x.settingName === settingName);

            if (!existingSetting) {
                const setting: Settings = {
                    settingName,
                    valueString,
                    valueDouble
                };
                await store.createSetting(setting);
            } else {
                existingSetting.valueString = valueString;
                existingSetting.valueDouble = valueDouble;
                await store.updateSetting(existingSetting.settingName, existingSetting);
            }

        } catch (e: any) {
            store.setError(e.message);
        }
    };

    // Helper function to convert string time to Dayjs object
    const convertToDayjs = (time: string): Dayjs => {
        return dayjs(time, "HH:mm");
    };

    // Helper function to convert Dayjs object to string time
    const convertToString = (time: Dayjs | null): string => {
        return time ? time.format("HH:mm") : "";
    };

    return (
        <LocalizationProvider dateAdapter={AdapterDayjs}>
            <Box sx={{ p: 4, maxWidth: 800, margin: '0 auto', alignItems: 'flex-start' }}>
                <Typography variant="h4" gutterBottom>
                    Settings Editor
                </Typography>

                <Divider sx={{ mb: 4 }} />

                <Stack spacing={3}>
                    {/* Delay Between Executions */}
                    <TextField
                        fullWidth
                        label="Delay between executions (ms)"
                        value={state.delayBetweenSets}
                        onChange={(evt: React.ChangeEvent<HTMLInputElement>) => {
                            const value = Number(evt.target.value);
                            handleSaveSetting(delayMs, "", value);
                            runInAction(() => {
                                state.delayBetweenSets = value;
                            });
                        }}
                        type="number"
                        variant="outlined"
                        slotProps={{
                            inputLabel: { shrink: true },
                        }}
                    />

                    {/* Audio Enabled Switch */}
                    <FormControlLabel
                        control={
                            <Switch
                                checked={state.enableAudio === 1}
                                onChange={(evt: React.ChangeEvent<HTMLInputElement>) => {
                                    const result = evt.target.checked ? 1 : 0;
                                    handleSaveSetting(isAudioEnabled, "", result);
                                    runInAction(() => {
                                        state.enableAudio = result;
                                    });
                                }}
                                color="primary"
                            />
                        }
                        label="Audio Enabled"
                    />

                    {/* Danger Pins Enabled Switch */}
                    <FormControlLabel
                        control={
                            <Switch
                                checked={state.enableDangerPins === 1}
                                onChange={(evt: React.ChangeEvent<HTMLInputElement>) => {
                                    const result = evt.target.checked ? 1 : 0;
                                    handleSaveSetting(isDangerEnabled, "", result);
                                    runInAction(() => {
                                        state.enableDangerPins = result;
                                    });
                                }}
                                color="secondary"
                            />
                        }
                        label="Danger Pins Enabled"
                    />

                    {/* Schedule Settings */}
                    <Box>
                        <Typography variant="h6" gutterBottom>
                            Schedule Settings
                        </Typography>
                        <Stack direction="row" spacing={2}>
                            <TimePicker
                                label="Schedule On"
                                value={convertToDayjs(state.onAt)}
                                onChange={(newValue) => {
                                    const timeString = convertToString(newValue);
                                    handleSaveSetting(timeOnAt, timeString, 0);
                                    runInAction(() => {
                                        state.onAt = timeString;
                                    });
                                }}
                                slots={{
                                    textField: TextField,
                                }}
                                slotProps={{
                                    textField: {
                                        fullWidth: true,
                                        variant: 'outlined',
                                    },
                                }}
                            />

                            <TimePicker
                                label="Schedule Off"
                                value={convertToDayjs(state.offAt)}
                                onChange={(newValue) => {
                                    const timeString = convertToString(newValue);
                                    handleSaveSetting(timeOffAt, timeString, 0);
                                    runInAction(() => {
                                        state.offAt = timeString;
                                    });
                                }}
                                slots={{
                                    textField: TextField,
                                }}
                                slotProps={{
                                    textField: {
                                        fullWidth: true,
                                        variant: 'outlined',
                                    },
                                }}
                            />
                        </Stack>
                    </Box>

                    {/* Audio Schedule Settings */}
                    <Box>
                        <Typography variant="h6" gutterBottom>
                            Audio Schedule Settings
                        </Typography>
                        <Stack direction="row" spacing={2}>
                            <TimePicker
                                label="Audio On"
                                value={convertToDayjs(state.audioOnAt)}
                                onChange={(newValue) => {
                                    const timeString = convertToString(newValue);
                                    handleSaveSetting(audioTimeOnAt, timeString, 0);
                                    runInAction(() => {
                                        state.audioOnAt = timeString;
                                    });
                                }}
                                slots={{
                                    textField: TextField,
                                }}
                                slotProps={{
                                    textField: {
                                        fullWidth: true,
                                        variant: 'outlined',
                                    },
                                }}
                            />

                            <TimePicker
                                label="Audio Off"
                                value={convertToDayjs(state.audioOffAt)}
                                onChange={(newValue) => {
                                    const timeString = convertToString(newValue);
                                    handleSaveSetting(audioTimeOffAt, timeString, 0);
                                    runInAction(() => {
                                        state.audioOffAt = timeString;
                                    });
                                }}
                                slots={{
                                    textField: TextField,
                                }}
                                slotProps={{
                                    textField: {
                                        fullWidth: true,
                                        variant: 'outlined',
                                    },
                                }}
                            />
                        </Stack>
                    </Box>

                    {/* Base File Path (Moved to Bottom) */}
                    <TextField
                        fullWidth
                        label="Base File Path"
                        value={state.audioFileLocation}
                        onChange={(evt: React.ChangeEvent<HTMLInputElement>) => {
                            const value = evt.target.value;
                            handleSaveSetting(fileBasePath, value, 0);
                            runInAction(() => {
                                state.audioFileLocation = value;
                            });
                        }}
                        variant="outlined"
                        slotProps={{
                            inputLabel: { shrink: true },
                        }}
                    />
                </Stack>
            </Box>
        </LocalizationProvider>
    );

});

export default SettingsEditor;
