import { useTranslation } from 'react-i18next';

import { Button, Popover, Tooltip } from '..';
import { TestTubeIcon, ToolboxIcon } from '../../assets';
import { appRoutes } from '../../lib/router';

export type ToolboxProps = {};

export const Toolbox: React.FC<ToolboxProps> = () => {
  const [t] = useTranslation('route');

  return (
    <div className="fixed left-4 bottom-4">
      <Popover.Root>
        <Popover.Trigger asChild>
          <Button compact variant="filled">
            <ToolboxIcon width={20} />
          </Button>
        </Popover.Trigger>
        <Popover.Content side="top">
          <div className="flex flex-row gap-1">
            <Tooltip.Root>
              <Tooltip.Trigger asChild>
                <Button compact as="a" href={appRoutes.componentTest} variant="light">
                  <TestTubeIcon width={12} />
                </Button>
              </Tooltip.Trigger>
              <Tooltip.Content>
                <span className="text-xs text-gray-100">{t('component_test.title')}</span>
              </Tooltip.Content>
            </Tooltip.Root>
          </div>
        </Popover.Content>
      </Popover.Root>
    </div>
  );
};
