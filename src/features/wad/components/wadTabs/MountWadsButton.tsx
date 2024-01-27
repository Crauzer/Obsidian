import { useTranslation } from 'react-i18next';
import { LuPackagePlus } from 'react-icons/lu';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';

import { Button, Icon, Tooltip } from '../../../../components';
import { appRoutes } from '../../../../lib/router';
import { composeUrlQuery } from '../../../../utils';
import { useMountWads } from '../../api';

export const MountWadsButton = () => {
  const [t] = useTranslation('mountedWads');
  const navigate = useNavigate();

  const mountWadsMutation = useMountWads();

  const handleMountWads = () => {
    mountWadsMutation.mutate(
      {},
      {
        onSuccess: ({ wadIds }) => {
          if (wadIds.length === 0) {
            return;
          }

          toast.success(t('mount.success', { count: wadIds.length }));

          navigate(composeUrlQuery(appRoutes.mountedWads, { wadId: wadIds[0] }));
        },
      },
    );
  };

  return (
    <Tooltip.Root>
      <Tooltip.Trigger asChild>
        <Button
          variant="filled"
          className="h-full rounded-l-none text-xl"
          onClick={() => handleMountWads()}
        >
          <Icon size="lg" icon={LuPackagePlus} />
        </Button>
      </Tooltip.Trigger>
      <Tooltip.Content side="bottom">{t('mount.tooltip')}</Tooltip.Content>
    </Tooltip.Root>
  );
};
