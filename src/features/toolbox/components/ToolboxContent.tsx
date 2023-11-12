import { useTranslation } from 'react-i18next';

import { TestTubeIcon } from '../../../assets';
import { Button, Tooltip } from '../../../components';
import { appRoutes } from '../../../lib/router';

export const ToolboxContent = () => {
  const [t] = useTranslation('route');

  return (
    <div className="flex flex-row gap-1">
      <Tooltip.Root>
        <Tooltip.Trigger asChild>
          <Button compact as="a" href={appRoutes.componentTest} variant="light">
            <TestTubeIcon width={12} />
          </Button>
        </Tooltip.Trigger>
        <Tooltip.Content>
          <span className="text-xs text-gray-100">{t('componentTest.title')}</span>
        </Tooltip.Content>
      </Tooltip.Root>
    </div>
  );
};
