import { Settings, SettingsFormData } from '..';

export const createSettingsFromFormData = (data: SettingsFormData): Settings => {
  return {
    defaultExtractionDirectory: data.defaultExtractionDirectory,
    defaultMountDirectory: data.defaultMountDirectory,
  };
};
