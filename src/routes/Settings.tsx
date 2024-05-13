import { Spinner } from '../components';
import { SettingsForm, createSettigsFormData, useSettings } from '../features/settings';

export default function Settings() {
  const settings = useSettings({ select: (data) => createSettigsFormData(data) });

  return (
    <div className="flex w-full flex-col gap-4 p-4">
      <h1 className="pl-2 text-3xl text-gray-50">Settings</h1>
      <div className="h-full w-full rounded border border-gray-600 bg-gray-900 p-8 shadow-inner">
        {settings.isLoading && <Spinner />}
        {settings.isSuccess && <SettingsForm defaultValues={settings.data} />}
      </div>
    </div>
  );
}
