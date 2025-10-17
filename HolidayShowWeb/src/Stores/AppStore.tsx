// src/Stores/AppStore.ts

import { action, computed, makeObservable, observable, runInAction } from "mobx";
import {
  Sets,
  SetSequences,
  Settings,
  Devices,
  DevicePatterns,
  DevicePatternSequences,
  DeviceIoPorts,
  DeviceEffects,
  EffectInstructionsAvailable,
  AudioOptions,
} from "../Clients/Api"; // Ensure all necessary types are imported
import { createStoreContextItem } from "../Common/StoreContextItem";
import { DisposableBase, IInitializable } from "../Types";
import { ApiStore, ApiStoreContextItem } from "./ApiStore";
import { DeviceStatusInfo } from "../Services/SignalRService";

export class AppStore extends DisposableBase implements IInitializable {
  private _apiStore: ApiStore;

  constructor(apiStore: ApiStore) {
    super();
    this._apiStore = apiStore;

    // Initialize observables and actions
    makeObservable(this, {
      // Observables for Lists
      sets: observable,
      devices: observable,
      devicePatterns: observable,
      devicePatternSequences: observable,
      deviceIoPorts: observable,
      deviceEffects: observable,
      effectInstructionsAvailables: observable,
      audioOptions: observable,
      setSequences: observable,
      settings: observable,
      deviceStatuses: observable,

      // Loading States
      isLoadingSets: observable,
      isLoadingDevices: observable,
      isLoadingDevicePatterns: observable,
      isLoadingDevicePatternSequences: observable,
      isLoadingDeviceIoPorts: observable,
      isLoadingDeviceEffects: observable,
      isLoadingEffectInstructionsAvailables: observable,
      isLoadingAudioOptions: observable,
      isLoadingSetSequences: observable,
      isLoadingSettings: observable,
      isInitLoading: observable,

      // Actions to Set Data
      setSets: action,
      setDevices: action,
      setDevicePatterns: action,
      setDevicePatternSequences: action,
      setDeviceIoPorts: action,
      setDeviceEffects: action,
      setEffectInstructionsAvailables: action,
      setAudioOptions: action,
      setSetSequences: action,
      setSettings: action,
      setDeviceStatuses: action,
      updateDeviceStatus: action,

      // Actions to Set Loading States
      setLoadingSets: action,
      setLoadingDevices: action,
      setLoadingDevicePatterns: action,
      setLoadingDevicePatternSequences: action,
      setLoadingDeviceIoPorts: action,
      setLoadingDeviceEffects: action,
      setLoadingEffectInstructionsAvailables: action,
      setLoadingAudioOptions: action,
      setLoadingSetSequences: action,
      setLoadingSettings: action,
      isInitLoadingSet: action,

      // Computed Properties
      currentSet: computed,

      // Error Handling
      error: observable,
      setError: action,
      clearError: action,

      // All Pins Actions
      createAllPinsOnSet: action,
    });
  }

  dispose() {
    // Clean up resources if needed
  }

  public initialize() {
    this.fetchAllData();
  }

  // ===========================
  // Observables for Lists
  // ===========================

  isInitLoading : boolean = true;
  isInitLoadingSet(isInitLoading: boolean) {
    this.isInitLoading = isInitLoading;
  }

  sets: Sets[] = [];
  devices: Devices[] = [];
  devicePatterns: DevicePatterns[] = [];
  devicePatternSequences: DevicePatternSequences[] = [];
  deviceIoPorts: DeviceIoPorts[] = [];
  deviceEffects: DeviceEffects[] = [];
  effectInstructionsAvailables: EffectInstructionsAvailable[] = [];
  audioOptions: AudioOptions[] = [];
  setSequences: SetSequences[] = [];
  settings: Settings[] = [];
  deviceStatuses: DeviceStatusInfo[] = [];

  // ===========================
  // Loading States
  // ===========================

  isLoadingSets: boolean = false;
  isLoadingDevices: boolean = false;
  isLoadingDevicePatterns: boolean = false;
  isLoadingDevicePatternSequences: boolean = false;
  isLoadingDeviceIoPorts: boolean = false;
  isLoadingDeviceEffects: boolean = false;
  isLoadingEffectInstructionsAvailables: boolean = false;
  isLoadingAudioOptions: boolean = false;
  isLoadingSetSequences: boolean = false;
  isLoadingSettings: boolean = false;

  // ===========================
  // Error Handling
  // ===========================

  error: string | null = null;

  setError(error: string) {
    this.error = error;
    console.error(error);
  }

  clearError() {
    this.error = null;
  }

  // ===========================
  // Actions to Set Data
  // ===========================

  setSets(sets: Sets[]) {
    this.sets = sets;
  }

  setDevices(devices: Devices[]) {
    this.devices = devices;
  }

  setDevicePatterns(devicePatterns: DevicePatterns[]) {
    this.devicePatterns = devicePatterns;
  }

  setDevicePatternSequences(devicePatternSequences: DevicePatternSequences[]) {
    this.devicePatternSequences = devicePatternSequences;
  }

  setDeviceIoPorts(deviceIoPorts: DeviceIoPorts[]) {
    this.deviceIoPorts = deviceIoPorts;
  }

  setDeviceEffects(deviceEffects: DeviceEffects[]) {
    this.deviceEffects = deviceEffects;
  }

  setEffectInstructionsAvailables(effectInstructionsAvailables: EffectInstructionsAvailable[]) {
    this.effectInstructionsAvailables = effectInstructionsAvailables;
  }

  setAudioOptions(audioOptions: AudioOptions[]) {
    this.audioOptions = audioOptions;
  }

  setSetSequences(setSequences: SetSequences[]) {
    this.setSequences = setSequences;
  }

  setSettings(settings: Settings[]) {
    this.settings = settings;
  }

  setDeviceStatuses(deviceStatuses: DeviceStatusInfo[]) {
    // Filter out invalid device statuses
    this.deviceStatuses = deviceStatuses.filter(ds => ds && typeof ds.deviceId === 'number');
  }

  updateDeviceStatus(deviceStatus: DeviceStatusInfo) {
    // Validate the device status
    if (!deviceStatus || typeof deviceStatus.deviceId !== 'number') {
      console.warn('Invalid device status received:', deviceStatus);
      return;
    }

    const existingIndex = this.deviceStatuses.findIndex(ds => ds.deviceId === deviceStatus.deviceId);
    if (existingIndex >= 0) {
      this.deviceStatuses[existingIndex] = deviceStatus;
    } else {
      this.deviceStatuses.push(deviceStatus);
    }
    // Trigger reactivity
    this.deviceStatuses = [...this.deviceStatuses];
  }

  // ===========================
  // Actions to Set Loading States
  // ===========================

  setLoadingSets(isLoading: boolean) {
    this.isLoadingSets = isLoading;
  }

  setLoadingDevices(isLoading: boolean) {
    this.isLoadingDevices = isLoading;
  }

  setLoadingDevicePatterns(isLoading: boolean) {
    this.isLoadingDevicePatterns = isLoading;
  }

  setLoadingDevicePatternSequences(isLoading: boolean) {
    this.isLoadingDevicePatternSequences = isLoading;
  }

  setLoadingDeviceIoPorts(isLoading: boolean) {
    this.isLoadingDeviceIoPorts = isLoading;
  }

  setLoadingDeviceEffects(isLoading: boolean) {
    this.isLoadingDeviceEffects = isLoading;
  }

  setLoadingEffectInstructionsAvailables(isLoading: boolean) {
    this.isLoadingEffectInstructionsAvailables = isLoading;
  }

  setLoadingAudioOptions(isLoading: boolean) {
    this.isLoadingAudioOptions = isLoading;
  }

  setLoadingSetSequences(isLoading: boolean) {
    this.isLoadingSetSequences = isLoading;
  }

  setLoadingSettings(isLoading: boolean) {
    this.isLoadingSettings = isLoading;
  }

  // ===========================
  // Computed Properties
  // ===========================

  get currentSet() {
    return this.settings.find((x) => x.settingName === "CurrentSet")?.valueDouble ?? 0;
  }

  // ===========================
  // Fetch Methods for Each List
  // ===========================

  // Fetch All Data
  async fetchAllData() {
    this.isInitLoadingSet(true);
    await Promise.all([
      this.fetchSets(),
      this.fetchDevices(),
      this.fetchDevicePatterns(),
      this.fetchDevicePatternSequences(),
      this.fetchDeviceIoPorts(),
      this.fetchDeviceEffects(),
      this.fetchEffectInstructionsAvailables(),
      this.fetchAudioOptions(),
      this.fetchSetSequences(),
      this.fetchSettings(),
    ]);
    this.isInitLoadingSet(false);
  }

  // Fetch Sets
  async fetchSets() {
    this.setLoadingSets(true);
    try {
      const sets = await this._apiStore.getApi().setsList();
      runInAction(() => {
        this.setSets(sets);
        this.clearError();
      });
    } catch (error: any) {
      this.setError("Failed to fetch sets: " + error.message);
      console.error("Failed to fetch sets:", error);
    } finally {
      runInAction(() => {
        this.setLoadingSets(false);
      });
    }
  }

  // Fetch Devices
  async fetchDevices() {
    this.setLoadingDevices(true);
    try {
      const devices = await this._apiStore.getApi().devicesList();
      runInAction(() => {
        this.setDevices(devices);
        this.clearError();
      });
    } catch (error: any) {
      this.setError("Failed to fetch devices: " + error.message);
      console.error("Failed to fetch devices:", error);
    } finally {
      runInAction(() => {
        this.setLoadingDevices(false);
      });
    }
  }

  // Fetch Device Patterns
  async fetchDevicePatterns() {
    this.setLoadingDevicePatterns(true);
    try {
      const devicePatterns = await this._apiStore.getApi().devicePatternsList();
      runInAction(() => {
        this.setDevicePatterns(devicePatterns);
        this.clearError();
      });
    } catch (error: any) {
      this.setError("Failed to fetch device patterns: " + error.message);
      console.error("Failed to fetch device patterns:", error);
    } finally {
      runInAction(() => {
        this.setLoadingDevicePatterns(false);
      });
    }
  }

  // Fetch Device Pattern Sequences
  async fetchDevicePatternSequences() {
    this.setLoadingDevicePatternSequences(true);
    try {
      const devicePatternSequences = await this._apiStore.getApi().devicePatternSequencesList();
      runInAction(() => {
        this.setDevicePatternSequences(devicePatternSequences);
        this.clearError();
      });
    } catch (error: any) {
      this.setError("Failed to fetch device pattern sequences: " + error.message);
      console.error("Failed to fetch device pattern sequences:", error);
    } finally {
      runInAction(() => {
        this.setLoadingDevicePatternSequences(false);
      });
    }
  }

  // Fetch Device Io Ports
  async fetchDeviceIoPorts() {
    this.setLoadingDeviceIoPorts(true);
    try {
      const deviceIoPorts = await this._apiStore.getApi().deviceIoPortsList();
      runInAction(() => {
        this.setDeviceIoPorts(deviceIoPorts);
        this.clearError();
      });
    } catch (error: any) {
      this.setError("Failed to fetch device Io ports: " + error.message);
      console.error("Failed to fetch device Io ports:", error);
    } finally {
      runInAction(() => {
        this.setLoadingDeviceIoPorts(false);
      });
    }
  }

  // Fetch Device Effects
  async fetchDeviceEffects() {
    this.setLoadingDeviceEffects(true);
    try {
      const deviceEffects = await this._apiStore.getApi().deviceEffectsList();
      runInAction(() => {
        this.setDeviceEffects(deviceEffects);
        this.clearError();
      });
    } catch (error: any) {
      this.setError("Failed to fetch device effects: " + error.message);
      console.error("Failed to fetch device effects:", error);
    } finally {
      runInAction(() => {
        this.setLoadingDeviceEffects(false);
      });
    }
  }

  // Fetch Effect Instructions Availables
  async fetchEffectInstructionsAvailables() {
    this.setLoadingEffectInstructionsAvailables(true);
    try {
      const effectInstructionsAvailables = await this._apiStore.getApi().effectInstructionsAvailablesList();
      runInAction(() => {
        this.setEffectInstructionsAvailables(effectInstructionsAvailables);
        this.clearError();
      });
    } catch (error: any) {
      this.setError("Failed to fetch effect instructions availables: " + error.message);
      console.error("Failed to fetch effect instructions availables:", error);
    } finally {
      runInAction(() => {
        this.setLoadingEffectInstructionsAvailables(false);
      });
    }
  }

  // Fetch Audio Options
  async fetchAudioOptions() {
    this.setLoadingAudioOptions(true);
    try {
      const audioOptions = await this._apiStore.getApi().audioOptionsList();
      runInAction(() => {
        this.setAudioOptions(audioOptions);
        this.clearError();
      });
    } catch (error: any) {
      this.setError("Failed to fetch audio options: " + error.message);
      console.error("Failed to fetch audio options:", error);
    } finally {
      runInAction(() => {
        this.setLoadingAudioOptions(false);
      });
    }
  }

  // Fetch Set Sequences
  async fetchSetSequences() {
    this.setLoadingSetSequences(true);
    try {
      const setSequences = await this._apiStore.getApi().setSequencesList();
      runInAction(() => {
        this.setSetSequences(setSequences);
        this.clearError();
      });
    } catch (error: any) {
      this.setError("Failed to fetch set sequences: " + error.message);
      console.error("Failed to fetch set sequences:", error);
    } finally {
      runInAction(() => {
        this.setLoadingSetSequences(false);
      });
    }
  }

  // Fetch Settings
  async fetchSettings() {
    this.setLoadingSettings(true);
    try {
      const settings = await this._apiStore.getApi().settingsList();
      runInAction(() => {
        this.setSettings(settings);
        this.clearError();
      });
    } catch (error: any) {
      this.setError("Failed to fetch settings: " + error.message);
      console.error("Failed to fetch settings:", error);
    } finally {
      runInAction(() => {
        this.setLoadingSettings(false);
      });
    }
  }

  // ===========================
  // Create, Update, Delete Actions
  // ===========================

  // Sets
  async createSet(data: Sets) {
    try {
      const newSet = await this._apiStore.getApi().setsCreate(data);
      await this.fetchSets();
      this.clearError();
      return this.sets.find((x) => x.setId === newSet.setId);
    } catch (error: any) {
      this.setError("Failed to create set: " + error.message);
      console.error("Failed to create set:", error);
    }
  }

  async updateSet(id: number, data: Sets) {
    try {
      await this._apiStore.getApi().setsUpdate(id, data);
      await this.fetchSets();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to update set: " + error.message);
      console.error("Failed to update set:", error);
    }
  }

  async deleteSet(id: number) {
    try {
      await this._apiStore.getApi().setsDelete(id);
      await this.fetchSets();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to delete set: " + error.message);
      console.error("Failed to delete set:", error);
    }
  }

  // Devices
  async createDevice(data: Devices) {
    try {
      await this._apiStore.getApi().devicesCreate(data);
      await this.fetchDevices();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to create device: " + error.message);
      console.error("Failed to create device:", error);
    }
  }

  async updateDevice(id: number, data: Devices) {
    try {
      await this._apiStore.getApi().devicesUpdate(id, data);
      await this.fetchDevices();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to update device: " + error.message);
      console.error("Failed to update device:", error);
    }
  }

  async deleteDevice(id: number) {
    try {
      await this._apiStore.getApi().devicesDelete(id);
      await this.fetchDevices();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to delete device: " + error.message);
      console.error("Failed to delete device:", error);
    }
  }

  // DevicePatterns
  async createDevicePattern(data: DevicePatterns) : Promise<DevicePatterns | undefined> {
    try {
      const newPattern = await this._apiStore.getApi().devicePatternsCreate(data);
      await this.fetchDevicePatterns();
      this.clearError();
      return newPattern;
    } catch (error: any) {
      this.setError("Failed to create device pattern: " + error.message);
      console.error("Failed to create device pattern:", error);
    }
  }

  async updateDevicePattern(id: number, data: DevicePatterns) {
    try {
      await this._apiStore.getApi().devicePatternsUpdate(id, data);
      await this.fetchDevicePatterns();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to update device pattern: " + error.message);
      console.error("Failed to update device pattern:", error);
    }
  }

  async deleteDevicePattern(id: number) {
    try {
      await this._apiStore.getApi().devicePatternsDelete(id);
      await this.fetchDevicePatterns();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to delete device pattern: " + error.message);
      console.error("Failed to delete device pattern:", error);
    }
  }

  // DevicePatternSequences
  async createDevicePatternSequence(deviceId: number, data: DevicePatternSequences) {
    try {
      const newItem = await this._apiStore.getApi().devicePatternSequencesCreate(deviceId, data);
      await this.fetchDevicePatternSequences();
      this.clearError();
      return newItem;
    } catch (error: any) {
      this.setError("Failed to create device pattern sequence: " + error.message);
      console.error("Failed to create device pattern sequence:", error);
    }
  }

  async updateDevicePatternSequence(id: number, data: DevicePatternSequences) {
    try {
      await this._apiStore.getApi().devicePatternSequencesUpdate(id, data);
      await this.fetchDevicePatternSequences();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to update device pattern sequence: " + error.message);
      console.error("Failed to update device pattern sequence:", error);
    }
  }

  async deleteDevicePatternSequence(id: number) {
    try {
      await this._apiStore.getApi().devicePatternSequencesDelete(id);
      await this.fetchDevicePatternSequences();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to delete device pattern sequence: " + error.message);
      console.error("Failed to delete device pattern sequence:", error);
    }
  }

  // DeviceIoPorts
  async createDeviceIoPort(data: DeviceIoPorts) {
    try {
      await this._apiStore.getApi().deviceIoPortsCreate(data);
      await this.fetchDeviceIoPorts();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to create device IO port: " + error.message);
      console.error("Failed to create device IO port:", error);
    }
  }

  async updateDeviceIoPort(id: number, data: DeviceIoPorts) {
    try {
      await this._apiStore.getApi().deviceIoPortsUpdate(id, data);
      await this.fetchDeviceIoPorts();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to update device IO port: " + error.message);
      console.error("Failed to update device IO port:", error);
    }
  }

  async deleteDeviceIoPort(id: number) {
    try {
      await this._apiStore.getApi().deviceIoPortsDelete(id);
      await this.fetchDeviceIoPorts();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to delete device IO port: " + error.message);
      console.error("Failed to delete device IO port:", error);
    }
  }

  // DeviceEffects
  async createDeviceEffect(data: DeviceEffects) {
    try {
      const effect = await this._apiStore.getApi().deviceEffectsCreate(data);
      await this.fetchDeviceEffects();
      this.clearError();
      return this.deviceEffects.find((x) => x.effectId === effect.effectId);
    } catch (error: any) {
      this.setError("Failed to create device effect: " + error.message);
      console.error("Failed to create device effect:", error);
    }
  }

  async updateDeviceEffect(id: number, data: DeviceEffects) {
    try {
      await this._apiStore.getApi().deviceEffectsUpdate(id, data);
      await this.fetchDeviceEffects();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to update device effect: " + error.message);
      console.error("Failed to update device effect:", error);
    }
  }

  async deleteDeviceEffect(id: number) {
    try {
      await this._apiStore.getApi().deviceEffectsDelete(id);
      await this.fetchDeviceEffects();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to delete device effect: " + error.message);
      console.error("Failed to delete device effect:", error);
    }
  }

  // EffectInstructionsAvailables
  async createEffectInstructionsAvailable(data: EffectInstructionsAvailable) {
    try {
      await this._apiStore.getApi().effectInstructionsAvailablesCreate(data);
      await this.fetchEffectInstructionsAvailables();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to create effect instructions available: " + error.message);
      console.error("Failed to create effect instructions available:", error);
    }
  }

  async updateEffectInstructionsAvailable(id: number, data: EffectInstructionsAvailable) {
    try {
      await this._apiStore.getApi().effectInstructionsAvailablesUpdate(id, data);
      await this.fetchEffectInstructionsAvailables();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to update effect instructions available: " + error.message);
      console.error("Failed to update effect instructions available:", error);
    }
  }

  async deleteEffectInstructionsAvailable(id: number) {
    try {
      await this._apiStore.getApi().effectInstructionsAvailablesDelete(id);
      await this.fetchEffectInstructionsAvailables();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to delete effect instructions available: " + error.message);
      console.error("Failed to delete effect instructions available:", error);
    }
  }

  // AudioOptions
  async createAudioOption(data: AudioOptions) {
    try {
      await this._apiStore.getApi().audioOptionsCreate(data);
      await this.fetchAudioOptions();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to create audio option: " + error.message);
      console.error("Failed to create audio option:", error);
    }
  }

  async updateAudioOption(id: number, data: AudioOptions) {
    try {
      await this._apiStore.getApi().audioOptionsUpdate(id, data);
      await this.fetchAudioOptions();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to update audio option: " + error.message);
      console.error("Failed to update audio option:", error);
    }
  }

  async deleteAudioOption(id: number) {
    try {
      await this._apiStore.getApi().audioOptionsDelete(id);
      await this.fetchAudioOptions();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to delete audio option: " + error.message);
      console.error("Failed to delete audio option:", error);
    }
  }

  // SetSequences
  async createSetSequence(data: SetSequences) {
    try {
      await this._apiStore.getApi().setSequencesCreate(data);
      await this.fetchSetSequences();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to create set sequence: " + error.message);
      console.error("Failed to create set sequence:", error);
    }
  }

  async updateSetSequence(id: number, data: SetSequences) {
    try {
      await this._apiStore.getApi().setSequencesUpdate(id, data);
      await this.fetchSetSequences();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to update set sequence: " + error.message);
      console.error("Failed to update set sequence:", error);
    }
  }

  async deleteSetSequence(id: number) {
    try {
      await this._apiStore.getApi().setSequencesDelete(id);
      await this.fetchSetSequences();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to delete set sequence: " + error.message);
      console.error("Failed to delete set sequence:", error);
    }
  }

  // Settings
  async createSetting(data: Settings) {
    try {
      await this._apiStore.getApi().settingsCreate(data);
      await this.fetchSettings();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to create setting: " + error.message);
      console.error("Failed to create setting:", error);
    }
  }

  async updateSetting(id: string, data: Settings) {
    try {
      await this._apiStore.getApi().settingsUpdate(id, data);
      await this.fetchSettings();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to update setting: " + error.message);
      console.error("Failed to update setting:", error);
    }
  }

  async deleteSetting(id: string) {
    try {
      await this._apiStore.getApi().settingsDelete(id);
      await this.fetchSettings();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to delete setting: " + error.message);
      console.error("Failed to delete setting:", error);
    }
  }

  async restartExecution() {
    try {
      await this._apiStore.getApi().settingsRestartExecutionUpdate();
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to restart execution: " + error.message);
      console.error("Failed to restart execution:", error);
    }
  }

  async updatePlaybackOption(playbackOption: number) {
    try {
      await this._apiStore.getApi().settingsPlaybackOptionUpdate(playbackOption);
      this.clearError();
    } catch (error: any) {
      this.setError("Failed to update playback option: " + error.message);
      console.error("Failed to update playback option:", error);
    }
  }

  // ===========================
  // All Pins On Set Creation
  // ===========================

  async createAllPinsOnSet(): Promise<number> {
    try {
      // Get IsDangerEnabled setting
      const isDangerEnabled = this.settings.find(s => s.settingName === 'IsDangerEnabled')?.valueDouble === 1;

      // Filter eligible pins: not hidden, respect danger setting
      const eligiblePins = this.deviceIoPorts.filter(pin => 
        !pin.isNotVisable && (isDangerEnabled || !pin.isDanger)
      );

      if (eligiblePins.length === 0) {
        throw new Error('No eligible pins found for All Pins On');
      }

      // Build DEVPINS metadata
      const devPinsList = eligiblePins.map(p => `${p.deviceId}:${p.commandPin}`).join(',');
      const instructionMetaData = `DEVPINS=${devPinsList}`; // GPIO On (ID 3) doesn't use DUR

      // Step 1: Find or Create DeviceEffect
      let effect = this.deviceEffects.find(e => 
        e.effectName === 'All Pins On' && 
        e.effectInstructionId === 3
      );

      if (effect) {
        // Update existing effect
        const updatedEffect = {
          ...effect,
          instructionMetaData: instructionMetaData,
          duration: 3600000
        };
        await this._apiStore.getApi().deviceEffectsUpdate(effect.effectId, updatedEffect);
        effect = updatedEffect;
      } else {
        // Create new effect
        const newEffect = {
          effectId: 0,
          effectName: 'All Pins On',
          instructionMetaData: instructionMetaData,
          duration: 3600000,
          effectInstructionId: 3,
          timeOn: null,
          timeOff: null
        };
        effect = await this._apiStore.getApi().deviceEffectsCreate(newEffect);
      }

      // Step 2: Find or Create Set
      let set = this.sets.find(s => s.setName === 'All Pins On');

      if (!set) {
        // Create new set
        const newSet = {
          setId: 0,
          setName: 'All Pins On',
          isDisabled: false
        };
        set = await this._apiStore.getApi().setsCreate(newSet);
      }

      // Step 3: Find or Create SetSequence
      let setSeq = this.setSequences.find(ss => 
        ss.setId === set!.setId && 
        ss.effectId === effect!.effectId
      );

      if (!setSeq) {
        // Check if there's any sequence for this set
        const existingSeq = this.setSequences.find(ss => ss.setId === set!.setId);
        
        if (existingSeq) {
          // Update existing sequence to point to our effect
          const updatedSeq = {
            ...existingSeq,
            effectId: effect!.effectId,
            devicePatternId: null,
            onAt: 0
          };
          await this._apiStore.getApi().setSequencesUpdate(existingSeq.setSequenceId, updatedSeq);
        } else {
          // Create new sequence
          const newSeq = {
            setSequenceId: 0,
            setId: set!.setId,
            onAt: 0,
            devicePatternId: null,
            effectId: effect!.effectId
          };
          await this._apiStore.getApi().setSequencesCreate(newSeq);
        }
      }

      // Step 4: Refresh store data
      await this.fetchDeviceEffects();
      await this.fetchSets();
      await this.fetchSetSequences();

      this.clearError();
      return set!.setId;
    } catch (error: any) {
      this.setError("Failed to create All Pins On set: " + error.message);
      console.error("Failed to create All Pins On set:", error);
      throw error;
    }
  }
}

// ===========================
// Create Store Context
// ===========================

export const AppStoreContextItem = createStoreContextItem<AppStore>(() => {
  const apiStore = ApiStoreContextItem.useStore();
  const appStore = new AppStore(apiStore);
  appStore.initialize(); // Ensure initialization
  return () => appStore;
});
