import { SettingsForm } from '../features/settings';

export default function Settings() {
  return (
    <div className="flex w-full flex-col gap-4 p-4">
      <h1 className="pl-2 text-3xl text-gray-50">Settings</h1>
      <div className="h-full w-full rounded border border-gray-600 bg-gray-900 px-6 py-4 shadow-inner">
        <SettingsForm />
      </div>
    </div>
  );
}
