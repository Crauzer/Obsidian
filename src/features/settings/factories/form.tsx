import type { Settings, SettingsFormData } from "..";

export const createSettigsFormData = (settings: Settings): SettingsFormData => {
  return {
    openDirectoryAfterExtraction: settings.openDirectoryAfterExtraction,
    defaultExtractionDirectory: settings.defaultExtractionDirectory,
    defaultMountDirectory: settings.defaultMountDirectory,
    leagueDirectory: settings.leagueDirectory,
  };
};

export const createSettingsFromFormData = (
  data: SettingsFormData,
): Settings => {
  return {
    openDirectoryAfterExtraction: data.openDirectoryAfterExtraction,
    defaultExtractionDirectory: data.defaultExtractionDirectory,
    defaultMountDirectory: data.defaultMountDirectory,
    leagueDirectory: data.leagueDirectory,
  };
};
